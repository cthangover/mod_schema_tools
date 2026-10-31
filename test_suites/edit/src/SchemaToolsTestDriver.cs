namespace Cthangover.Tools.Schemas.Tests
{
    using Cthangover.Mods.Schemas;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Nodes;
    using Cthangover.Core.Bootstrap;
    using Cthangover.Core.Cards.StatusEffect;
    using Cthangover.Core.Items;
    using Cthangover.Core.Localization;
    using Cthangover.Core.Mods;
    using Cthangover.Core.Quests;
    using Cthangover.Mods.Skills;
    using Cthangover.Core.Mods.Resolvers;
    using Cthangover.Core.UI.Tool.ScenarioEditor;
    using Cthangover.Core.Utils;
    using Cthangover.Mods.Equipment;
    using Cthangover.Mods.Interface.Tool;
    using Cthangover.Mods.Stats;
    using Cthangover.Tools.Services;
    using Godot;

    /// <summary>
    /// Drives the Data Editor window and the command palette like an author: a new item gets
    /// gear fields that two different mods described, the game sees it as gear right after
    /// Save, and the palette writes and checks a stats line. Everything the test writes is
    /// removed again, the files it changed are restored byte for byte.
    /// </summary>
    public partial class SchemaToolsTestDriver : Node
    {
        private const float TickSeconds = 0.15f;
        private const float TimeoutSeconds = 60f;
        private const string Mod = "rpg_plus_game";
        private const string NewFile = "items/rpg_plus_game.json";
        private const string NewId = "rpg_plus_game/new_item";
        private const string ScenarioKey = "rpg_plus_game:scenarios/rpg_plus/events/seer.scenario";
        private const string Owner = "classic_rpg_pack";
        private const string OwnerFile = "items/classic/classic_items.json";
        private const string PatchFile = "patches/item/rpg_plus_game.json";
        private const string Dagger = "classic/dagger";
        private const string QuestId = "rpg_plus_game/new_quest";
        private const string SkillId = "rpg_plus_game/new_skill";
        private const string StatusId = "rpg_plus_game/new_status";
        private static readonly string[] KindFiles = { "quests/rpg_plus_game.json", "characters/skills/rpg_plus_game.json", "status_effects/rpg_plus_game.json" };

        private readonly List<string> failures = new();
        private readonly Dictionary<string, string> backups = new();
        private readonly List<string> createdFiles = new();
        private int passed;
        private int step;
        private double elapsed;
        private bool newFileExisted;
        private bool itemsDirExisted;
        private bool patchExisted;
        private string patchBackup;
        private string ownerBefore;
        private DataEditorWindow editor;
        private CommandPaletteWindow palette;
        private ScenarioEditorWindow scenarios;

        public SchemaToolsTestDriver()
        {
            Name = "SchemaToolsTestDriver";
            var timer = new Timer { WaitTime = TickSeconds, Autostart = true };
            AddChild(timer);
            timer.Timeout += Tick;
        }

        private void Tick()
        {
            if (step < 0)
                return;
            elapsed += TickSeconds;
            if (elapsed > TimeoutSeconds)
            {
                Fail($"timeout at step {step}");
                Cleanup();
                Finish();
                return;
            }
            try
            {
                RunStep();
            }
            catch (System.Exception ex)
            {
                Fail($"step {step}: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                Cleanup();
                Finish();
            }
        }

        private void RunStep()
        {
            switch (step)
            {
                case 0:
                    if (!Boot.IsReady)
                        return;
                    foreach (var file in ModFileService.ListFiles(Mod, "locale", ".properties"))
                        backups[file] = ModFileService.ReadText(Mod, file);
                    newFileExisted = ModFileService.ReadText(Mod, NewFile) != null;
                    patchExisted = ModFileService.ReadText(Mod, PatchFile) != null;
                    patchBackup = ModFileService.ReadText(Mod, PatchFile);
                    ownerBefore = ModFileService.ReadText(Owner, OwnerFile);
                    itemsDirExisted = Directory.Exists(Path.GetDirectoryName(ModFileService.GetWritablePath(Mod, NewFile)) ?? string.Empty);
                    Check(!newFileExisted, "the game mod has no items file of its own yet");
                    // The quest, skill and status files of the game mod: an existing one is restored, a missing one removed.
                    foreach (var kindFile in KindFiles)
                    {
                        var text = ModFileService.ReadText(Mod, kindFile);
                        if (text != null)
                            backups[kindFile] = text;
                        else
                            createdFiles.Add(kindFile);
                    }
                    editor = ToolWindow.Open<DataEditorWindow>();
                    step++;
                    return;
                case 1:
                    if (!editor.IsInsideTree())
                        return;
                    var kinds = SchemaCatalog.Current.Kinds;
                    Check(new[] { "stat", "archetype", "equip_slot", "item", "character", "action", "shop", "quest", "skill", "status_effect" }.All(kinds.ContainsKey), "the editor lists the kinds of the schemas, quests, skills and statuses included");
                    editor.OpenKind("item");
                    editor.OpenMod(Mod);
                    Check(editor.KindId == "item" && editor.ModId == Mod, "items of the game mod are open");
                    editor.AddNew();
                    Check(editor.SelectedId == NewId, $"New adds {NewId} (got {editor.SelectedId})");
                    // Equip is a picker of the equip types the slots accept (plan 14.G5): the sword is picked, not typed.
                    var equip = editor.FindChild("Picker_Equip", true, false) as OptionButton;
                    var sword = equip == null ? -1 : Enumerable.Range(0, equip.ItemCount).FirstOrDefault(i => equip.GetItemText(i) == "sword", -1);
                    Check(sword >= 0, "Equip offers the sword in its picker");
                    if (sword >= 0)
                    {
                        equip.Select(sword);
                        equip.EmitSignal(OptionButton.SignalName.ItemSelected, sword);
                    }
                    var list = editor.FindChild("List_Traits", true, false);
                    Check(list != null, "the equipment mod's Traits field is a list editor in the item form");
                    (list?.FindChild("Add", true, false) as Button)?.EmitSignal(BaseButton.SignalName.Pressed);
                    step++;
                    return;
                case 2:
                    var card = editor.FindChild("List_Traits", true, false)?.FindChild("Card0", true, false);
                    Check(card != null, "Add made a trait card");
                    var picker = card?.FindChild("Picker_Stat", true, false) as OptionButton;
                    var index = Enumerable.Range(0, picker?.ItemCount ?? 0).FirstOrDefault(i => picker.GetItemText(i).EndsWith(" atk") || picker.GetItemText(i) == "atk");
                    Check(picker != null && index > 0, "the trait's stat picker offers the pack's stats");
                    picker?.Select(index);
                    picker?.EmitSignal(OptionButton.SignalName.ItemSelected, index);
                    Type(card, "Text_Add", "3");
                    var data = editor.Document.Find(NewId)?.Data;
                    Check(data?["Equip"]?.ToString() == "sword", "the form wrote the equip type");
                    Check(data?["Traits"]?[0]?["Stat"]?.ToString() == "atk" && data?["Traits"]?[0]?["Add"]?.ToString() == "3", $"the form wrote the trait: {data?["Traits"]?.ToJsonString()}");
                    Check(editor.Save(), "Save writes the mod");
                    Check(EquipmentService.IsEquipment(NewId), "the game knows the new item as gear right after saving");
                    var traits = ExtraData.Traits(EquipmentService.Item(NewId)?.ExtraProperties, NewId);
                    Check(traits.Count == 1 && traits[0].Stat == "atk" && traits[0].Add == 3, "with its trait");
                    var written = ModFileService.ReadText(Mod, NewFile) ?? string.Empty;
                    Check(written.Contains("\"Kind\": \"item\"") && !written.Contains("\r"), "the new file declares its kind and has LF lines");
                    palette = ToolWindow.Open<CommandPaletteWindow>();
                    scenarios = ToolWindow.Open<ScenarioEditorWindow>();
                    step++;
                    return;
                case 3:
                    // The RPG schemas: the object kind lists village_pack's objects and shows their form fields (read only).
                    Check(new[] { "rpg_object", "rpg_placement", "rpg_map" }.All(SchemaCatalog.Current.Kinds.ContainsKey), "the Data Editor lists the RPG kinds");
                    editor.OpenKind("rpg_object");
                    editor.OpenMod("village_pack");
                    Check(editor.Document.Records.Count > 0 && editor.Editor("Form") != null, $"village_pack's objects open in the RPG object schema ({editor.Document.Records.Count})");
                    // The Data Editor edits another mod's dagger: the change is a patch of the edited mod.
                    editor.OpenKind("item");
                    editor.OpenMod(Mod);
                    Check(editor.Document.FindForeign(Dagger) != null, "the dagger of a dependency is listed as a patch candidate");
                    editor.Select(Dagger);
                    Check(editor.SelectedId == Dagger, "the dagger is selected");
                    editor.Editor("Cost")?.Load(JsonValue.Create(25));
                    editor.ApplyForm();
                    var entry = editor.Document.PatchOf(Dagger);
                    Check(entry?["Cost"]?.ToString() == "25" && entry.Count == 2, $"the price becomes a minimal patch entry ({entry?.ToJsonString()})");
                    Check(editor.Save(), "Save writes the patch");
                    Check(ModFileService.ReadText(Mod, PatchFile)?.Contains("\"Cost\": 25") == true, "the patch file of the edited mod holds it");
                    Check(ModFileService.ReadText(Owner, OwnerFile) == ownerBefore, "the owner's file stays byte for byte");
                    Check(Entities.Collect<ItemInfo>().TryGetValue(Dagger, out var priced) && priced.Cost == 25, "the game sees the patched price at once");
                    editor.Document.ResetForeign(editor.Document.FindForeign(Dagger));
                    Check(editor.Save(), "Reset to inherited and Save");
                    Check(ModFileService.ReadText(Mod, PatchFile) == null || !patchExisted, "the patch file is gone again");
                    Check(Entities.Collect<ItemInfo>().TryGetValue(Dagger, out var back) && back.Cost == 18, "the game sees the owner's price again");
                    step++;
                    return;
                case 4:
                    if (!palette.IsInsideTree() || !scenarios.IsInsideTree())
                        return;
                    scenarios.OpenScenario(ScenarioKey);
                    palette.Choose("action:stat.add");
                    palette.SetValue("stat", "str");
                    palette.SetValue("value", "2");
                    Check(palette.Line == "action stat.add stat=str value=2", $"the palette writes the line ({palette.Line})");
                    Check((palette.FindChild("Issues", true, false) as Label)?.Text.StartsWith("✔") == true, "and finds nothing wrong with it");
                    palette.SetValue("stat", "strenght");
                    Check((palette.FindChild("Issues", true, false) as Label)?.Text.Contains("✖") == true, "a misspelt stat is reported as an error");
                    palette.SetValue("stat", "str");
                    // The palette's flag field: a story flag, or the object's own flag with the self. scope.
                    palette.Choose("command:flag");
                    palette.SetValue("name", "self.picked");
                    Check(palette.Line == "flag self.picked 1", $"the palette writes a self. flag ({palette.Line})");
                    Check((palette.FindChild("Issues", true, false) as Label)?.Text.StartsWith("✔") == true, $"and accepts it ({(palette.FindChild("Issues", true, false) as Label)?.Text})");
                    palette.SetValue("name", "self.");
                    Check((palette.FindChild("Issues", true, false) as Label)?.Text.Contains("✖") == true, "a bare self. is an error");
                    palette.SetValue("name", "pick ed");
                    Check((palette.FindChild("Issues", true, false) as Label)?.Text.Contains("✖") == true, "a name with a space is an error");
                    palette.Choose("action:stat.add");
                    palette.SetValue("stat", "str");
                    palette.SetValue("value", "2");
                    step++;
                    return;
                case 5:
                    var tabs = scenarios.FindChildren("*", "", true, false).OfType<ScenarioEditTabs>().FirstOrDefault();
                    if (tabs == null || !tabs.HasActiveFile)
                    {
                        if (elapsed > 30)
                            Fail("the scenario editor did not open the seer's scenario");
                        else
                            return;
                    }
                    Check(palette.InsertIntoEditor(), "Insert puts the line into the open scenario");
                    Check(tabs?.CurrentText.Contains("action stat.add stat=str value=2") == true, "the editor's text has the line");
                    Check(ModFileService.ReadText(Mod, "scenarios/rpg_plus/events/seer.scenario")?.Contains("stat.add") == false, "nothing is written to disk before the author saves");
                    step++;
                    return;
                case 6:
                    CreateQuestSkillStatus();
                    Cleanup();
                    Finish();
                    return;
            }
        }

        /// <summary>
        /// Plan 14.G13: a quest with two steps (the step texts typed in each language), a skill with a picker for its type,
        /// rarity and granted action card, and a status with a picker for its kind and behaviour. Each is saved and the game
        /// must know it at once: the quest's steps and texts, the skill's kind and action, the status's kind and duration.
        /// </summary>
        private void CreateQuestSkillStatus()
        {
            var language = LanguageCode.Normalize(TranslationServer.GetLocale()) ?? "ru";

            editor.OpenKind("quest");
            editor.OpenMod(Mod);
            editor.AddNew();
            Check(editor.SelectedId == QuestId, $"New adds the quest {QuestId} (got {editor.SelectedId})");
            Type(editor, "Text_Name_" + language, "Тестовое задание");
            var stages = editor.FindChild("Stages_StatusToDescription", true, false);
            Check(stages != null, "the quest's steps are a step editor in the Data Editor");
            var add = stages?.FindChild("Add", true, false) as Button;
            add?.EmitSignal(BaseButton.SignalName.Pressed);
            add?.EmitSignal(BaseButton.SignalName.Pressed);
            Type(editor, "StageText_0_" + language, "Первый шаг");
            Type(editor, "StageText_1_" + language, "Второй шаг");
            editor.ApplyForm();
            var steps = editor.Document.Find(QuestId)?.Data?["StatusToDescription"] as JsonObject;
            Check(steps?.Count == 2 && steps["0"] != null && steps["1"] != null, $"the form writes two steps ({steps?.ToJsonString()})");
            Check(editor.Save(), "Save writes the quest");
            var quest = QuestFactory.Instance.GetAll().FirstOrDefault(q => q.ID == QuestId);
            Check(quest != null && quest.StatusToDescription.Count == 2, "the game knows the quest with its two steps");
            Check(quest != null && quest.StatusToDescription.TryGetValue(1, out var second) && TranslationServer.Translate(second).ToString() == "Второй шаг",
                "and the text of the second step");

            editor.OpenKind("skill");
            editor.OpenMod(Mod);
            editor.AddNew();
            Check(editor.SelectedId == SkillId, $"New adds the skill {SkillId} (got {editor.SelectedId})");
            Type(editor, "Text_Name_" + language, "Тестовый навык");
            Check(SelectByText(editor.FindChild("Picker_Type", true, false) as OptionButton, "Active"), "the skill's type is a picker with Active");
            var action = editor.FindChild("Picker_GrantsAction", true, false) as OptionButton;
            Check(action != null && action.ItemCount > 1, "the granted action card is a picker of the actions");
            if (action != null && action.ItemCount > 1)
            {
                action.Select(1);
                action.EmitSignal(OptionButton.SignalName.ItemSelected, 1);
            }
            editor.ApplyForm();
            Check(editor.Save(), "Save writes the skill");
            Check(Entities.Collect<SkillInfo>().TryGetValue(SkillId, out var skill) && skill.SkillType == SkillType.Active
                && !string.IsNullOrEmpty(skill.GrantsAction) && TranslationServer.Translate(skill.Name).ToString() == "Тестовый навык",
                "the game knows the skill with its type, its action card and its name");

            editor.OpenKind("status_effect");
            editor.OpenMod(Mod);
            editor.AddNew();
            Check(editor.SelectedId == StatusId, $"New adds the status {StatusId} (got {editor.SelectedId})");
            Type(editor, "Text_Name_" + language, "Тестовое оглушение");
            Check(SelectByText(editor.FindChild("Picker_Type", true, false) as OptionButton, "Stun"), "the status's kind is a picker with Stun");
            Type(editor, "Text_Duration", "2");
            editor.ApplyForm();
            Check(editor.Save(), "Save writes the status");
            Check(Entities.Collect<StatusEffectInfo>().TryGetValue(StatusId, out var status) && status.Type == StatusEffectType.Stun
                && status.Duration == 2 && status.Actions == "basic/stun", "the game knows the status with its kind, duration and behaviour");
        }

        private static bool SelectByText(OptionButton picker, string text)
        {
            if (picker == null)
                return false;
            var index = Enumerable.Range(0, picker.ItemCount).FirstOrDefault(i => picker.GetItemText(i) == text, -1);
            if (index < 0)
                return false;
            picker.Select(index);
            picker.EmitSignal(OptionButton.SignalName.ItemSelected, index);
            return true;
        }

        private static void Type(Node root, string name, string text)
        {
            if (root?.FindChild(name, true, false) is not LineEdit edit)
                return;
            edit.Text = text;
            edit.EmitSignal(LineEdit.SignalName.TextChanged, text);
        }

        private void Cleanup()
        {
            if (step < 0)
                return;
            foreach (var window in new Node[] { editor, palette, scenarios }.Where(w => w != null && IsInstanceValid(w)))
                window.QueueFree();
            var path = ModFileService.GetWritablePath(Mod, NewFile);
            if (!newFileExisted && path != null && File.Exists(path))
                File.Delete(path);
            // The save created the folder too; only that empty folder of this run goes away.
            var folder = path == null ? null : Path.GetDirectoryName(path);
            if (!itemsDirExisted && folder != null && Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                Directory.Delete(folder);
            foreach (var created in createdFiles)
                ModFileService.DeleteFile(Mod, created);
            foreach (var backup in backups)
                ModFileService.WriteText(Mod, backup.Key, backup.Value);
            if (!patchExisted)
                ModFileService.DeleteFile(Mod, PatchFile);
            else if (patchBackup != null)
                ModFileService.WriteText(Mod, PatchFile, patchBackup);
            ModManager.Instance.GetResolver<EntityResolver>()?.ClearCache();
            ModManager.Instance.GetResolver<FileListResolver>()?.ClearCache();
            GameEntityIndex.Refresh();
            LocaleFileService.ReloadGame();
        }

        private void Check(bool condition, string what)
        {
            if (condition)
            {
                passed++;
                GameLogger.Log("TEST", $"[PASS] {what}");
            }
            else
            {
                Fail(what);
            }
        }

        private void Fail(string what)
        {
            failures.Add(what);
            GameLogger.Log("TEST", $"[FAIL] {what}", LogLevel.Error);
        }

        private void Finish()
        {
            step = -1;
            GameLogger.Log("TEST", $"[TEST_RESULT] name=schema_tools_edit passed={passed} failed={failures.Count}");
            Cthangover.Core.Bootstrap.ApplicationExit.Quit(GetTree(), failures.Count == 0 ? 0 : 1);
        }
    }
}
