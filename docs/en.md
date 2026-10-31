# Schema Tools — the Data Editor and the command palette

Schema Tools are two windows for authors, built entirely from the descriptions that mods ship as schemas (see the Schemas mod):

- the **Data Editor** edits entities of any described kind — stats, classes, items, characters, battle actions, shops, recipes — in forms;
- the **command palette** writes a scenario line from a form, checks it and puts it into the scenario editor.

Neither window knows any kind or action in advance: whatever the loaded mods describe appears in them. Both open from the tool box on the game's toolbar, while the game is running.

## The Data Editor

Pick a mod at the top, a kind of data on the left and an entry in the middle; the form is on the right. Here is Classic RPG Rules' iron sword in RPG Plus:

![The Data Editor: mod classic_rpg_pack, the kinds on the left (rpg_tileset 10, Stats 11, Archetypes 3, Equipment slots 3, Items 20, Characters 5, Battle actions 7, Shops 1, Crafting station 1, Recipe 3), its nine items in the middle with Iron sword selected, and the form on the right: Id, Name and Description with a text in each of seven languages, Cost 30, Picture, Item types TargetUsed, Item action action/equip, Equip type sword and a trait on the stat Attack.](assets/data-editor-item.jpg)

The form gathers the fields of several mods into one: Id, name, description, cost, picture, item types and item action are described by Items Engine; **Equip type**, the **Traits** list (a stat, an amount) and the classes allowed to wear it are added by Equipment. A kind that two mods describe is still one form.

- **Texts** (`Name`, `Description`) are shown as their translation key and a line per language of the mod; they are written into the mod's own locale files.
- **References** (a stat, an item, a class) are lists of the ids the game knows.
- **Lists** such as traits have a card per element with its own fields.
- Properties no schema describes are kept and shown under other fields as JSON.

The toolbar: **New** (Ctrl+N) adds an entry with the schema's defaults, **Copy** (Ctrl+D) duplicates the selected one, **Delete** asks first, **Undo**/**Redo** (Ctrl+Z / Ctrl+Y or Ctrl+Shift+Z) step through the changes, **Save** (Ctrl+S) writes only the changed files, with LF line endings. Right after Save the running game sees the change: a new item can be given and worn without a restart.

Only mods that can be written to are in the list at the top. The list shows the entries of the chosen mod and, with **Entries of other mods** ticked, the entries of its dependencies, marked `↗ owner`. Editing one of those writes a patch in the chosen mod (`patches/<kind>/<mod>.json`) with only the values you changed; the owner's file stays byte for byte the same. **Reset to inherited** removes the patch, and **↺** next to a field removes your value of that field.

## The command palette

Pick an action or a command on the left (search by words such as "damage", "item", "check"), fill the form, and the line appears under it:

![The palette with Change a stat for good selected: "Described by the mod stats_base", Stat Strength · str, Value 2, Who empty, the line "action stat.add stat=str value=2" and "The line is fine".](assets/palette-line.jpg)

The line is checked as you type, with the same rules as the Validator. A misspelt stat:

![The same form with the stat strenght: the line "action stat.add stat=strenght value=2" and the error "'stat.add': 'stat' names stat 'strenght', which no loaded mod defines".](assets/palette-misspelt.jpg)

**Insert** puts the line into the scenario open in the scenario editor, below the caret; **Copy** puts it on the clipboard. The seer's scenario of RPG Plus after Insert:

![The scenario editor with seer.scenario marked as changed (seer *): line 2 is now "action stat.add stat=str value=2", between the seer's greeting and the luck check.](assets/scenario-inserted.jpg)

A flag field has a **self.** check box: ticked, the line writes the flag of the object whose dialog is running (`flag self.picked 1`). A bare `self.` with no name is shown as an error under the line. **Insert** does not stop on it, so fix the name first.

Nothing is written to disk until the author presses **Save** in the scenario editor. The scenario text stays the source of truth: the palette only helps to write it.

## If something goes wrong

1. **A kind or an action is missing.** No loaded mod describes it in a schema; the Schemas page explains how to add one.
2. **The mod list is empty.** No mod can be written to (only mods installed as folders can be; packed ones cannot).
3. **Insert says no scenario is open.** Open one in the scenario editor first, or use Copy.
4. **A field is a plain text box.** Its schema type is text or unknown.
