# Schema Tools — 数据编辑器与命令面板

Schema Tools 是给作者用的两个窗口，完全由各模组作为模式（见 Schemas 模组）附带的描述构建：

- **数据编辑器**用表单编辑任何已描述类型的实体——属性、职业、物品、角色、战斗动作、商店、配方；
- **命令面板**从表单写出剧本行，校验它，再放进剧本编辑器。

两个窗口都不预先知道任何类型或动作：已加载模组描述了什么，它们就显示什么。两者都从游戏工具栏的工具箱打开，在游戏运行时使用。

## 数据编辑器

顶部选模组，左侧选数据类型，中间选条目，表单在右侧。下面是 RPG Plus 里 Classic RPG Rules 的铁剑：

![数据编辑器：模组 classic_rpg_pack，左侧是类型（rpg_tileset 10、Stats 11、Archetypes 3、Equipment slots 3、Items 20、Characters 5、Battle actions 7、Shops 1、Crafting station 1、Recipe 3），中间是它的九个物品，选中 Iron sword，右侧是表单：Id、Name 和 Description 各带七种语言的文本，Cost 30、Picture、Item types TargetUsed、Item action action/equip、Equip type sword，以及 Attack 属性上的一条特性。](assets/data-editor-item.jpg)

表单把几个模组的字段汇成一张：Id、名称、描述、价格、图片、物品类型和物品动作由 Items Engine 描述；**Equip type**、**Traits** 列表（一个属性、一个数值）以及允许穿戴它的职业由 Equipment 添加。两个模组描述同一个类型，仍然只有一张表单。

- **文本**（`Name`、`Description`）按各自的翻译键显示，每种语言一行；它们写入模组自己的本地化文件。
- **引用**（一个属性、物品或职业）是游戏已知 id 的列表。
- **列表**（如 traits）为每个元素提供一张带各自字段的卡片。
- 没有模式描述的属性会被保留，并作为 JSON 显示在其他字段中。

工具栏：**新建**（Ctrl+N）按模式的默认值添加条目，**复制**（Ctrl+D）复制选中的条目，**删除**会先询问，**撤销**/**重做**（Ctrl+Z / Ctrl+Y 或 Ctrl+Shift+Z）在改动之间前后走，**保存**（Ctrl+S）只写入改动的文件，行尾为 LF。保存之后运行中的游戏立刻看到变化：新物品无需重启就能给出并装备。

列表显示所选模组的条目；勾选 **Entries of other mods** 后，也显示其依赖模组的条目，并标记 `↗ 所有者`。编辑这类条目会在所选模组中写入补丁（`patches/<类型>/<模组>.json`），只包含改动的值；所有者的文件逐字节保持不变。**Reset to inherited** 删除补丁，字段旁的 **↺** 删除你对该字段的值。

## 命令面板

在左侧选一个动作或命令（可按「伤害」「物品」「check」之类的词搜索），填好表单，行就出现在下面：

![选中 Change a stat for good 的面板：「由模组 stats_base 描述」、Stat Strength · str、Value 2、Who 为空、行「action stat.add stat=str value=2」以及「这一行没有问题」。](assets/palette-line.jpg)

输入时就会校验这一行，规则与 Validator 相同。属性名打错了：

![用 strenght 作为属性的同一张表单：行「action stat.add stat=strenght value=2」和错误「'stat.add'：'stat' 指向的属性 'strenght' 没有任何已加载模组定义」。](assets/palette-misspelt.jpg)

**插入**会把这一行放入剧本编辑器中打开的剧本、光标所在位置；**复制**则放进剪贴板。RPG Plus 里女预言者的剧本在插入之后：

![剧本编辑器中标记为已修改的 seer.scenario（seer *）：第 2 行现在是「action stat.add stat=str value=2」，位于女预言者的问候和运气检定之间。](assets/scenario-inserted.jpg)

`flag` 字段带有 **self.** 复选框：勾选后，该行写入正在运行对话的对象的标志（`flag self.picked 1`）。没有名称的单独 `self.` 会在行下以错误显示；**插入** 不会因此停止，因此请先修正名称。

在作者于剧本编辑器里按**保存**之前，什么都不会写到磁盘上。剧本文本始终是唯一的事实来源：面板只是帮忙写它。

## 出问题了怎么办

1. **缺了某个类型或动作。** 没有已加载模组在模式里描述它；Schemas 页面说明了如何添加描述。
2. **模组列表是空的。** 没有可写的模组（只能写入以文件夹安装的模组；打包的不能写）。
3. **插入时提示没有打开的剧本。** 先在剧本编辑器里打开一个，或者用复制。
4. **某个字段是普通文本框。** 它在模式里的类型是 text 或未知。
