# Assets/Scripts/Ability / AGENTS

## 模块概览
`Ability/` 负责当前项目的战斗玩法与连招编辑。核心心智模型如下：

1. `ActorData.ComboGraph` 决定角色使用哪张连招图。
2. `ActorComboGraphSO` 持有整张图的节点集合。
3. `AbilityNode` 描述一个连招节点、可跳转目标、优先级和绑定行为。
4. `AbilityBehavior` / `BehaviorBase` 负责在固定逻辑帧里执行动作、攻击窗口、受击窗口。
5. `AbilityAction` / `AbilityAttack` / `AbilityCondition` 负责更细粒度的帧行为。
6. `Ability.Editor.Combo` 负责连招图的可视化编辑、保存和运行时高亮。

## 关键数据类型
| 类型 | 文件 | 作用 |
|------|------|------|
| `ActorData` | `Assets/Scripts/Ability/Actor/ActorData.cs` | 角色配置，包含 Prefab、物理参数、`ComboGraph` |
| `ActorComboGraphSO` | `Assets/Scripts/Ability/Combo/ActorComboGraphSO.cs` | 连招图资产，本体只保存 `Nodes` |
| `AbilityNode` | `Assets/Scripts/Ability/AbilityNode.cs` | 节点数据：`Id`、`Childs`、`conditions`、`Priority`、`Behavior` |
| `AbilityBehavior` | `Assets/Scripts/Ability/Behavior/AbilityBehavior.cs` | 可循环行为、输入键、格挡角等公共行为数据 |
| `BehaviorBase` | `Assets/Scripts/Ability/BehaviorBase.cs` | 统一的行为生命周期和动作窗口执行器 |
| `AbilityAction` | `Assets/Scripts/Ability/AbilityAction.cs` | 按帧执行的动作单元 |
| `AbilityAttack` | `Assets/Scripts/Ability/AbilityAttack.cs` | 攻击窗口与 HitBox 生命周期 |
| `AbilityCondition` | `Assets/Scripts/Ability/AbilityCondition.cs` | 节点条件基类，编辑器仍支持维护 |

## 当前运行时模型

### 入口
- 文件：`Assets/Scripts/Ability/Actor/BehaviorComp.cs`
- `BehaviorComp.Enter(IEntity model)` 会读取 `model.GetComp<PlayerDataComp>().Data.ComboGraph`。
- 如果角色没有配置 `ComboGraph`，运行时直接报错并停止初始化连招。

### 图加载
- `LoadComboGraph(ActorComboGraphSO comboGraph)` 会清空 `nodeList`、`nodeDict`、`behaviorsList`、`hurtNodeDict`。
- 节点来源只来自 `comboGraph.Nodes`。
- 节点会按 `AbilityNode.Id` 排序后缓存。
- 行为缓存只从每个节点的 `node.Behavior` 去重收集，不再读取旧版 `LocalBehaviors`。
- `InitBehaviors()` 会调用：
  - `behavior.Init()`
  - 每个 `AbilityAction.Init()`
  - `AbilityBehaviorAttack.Attacks` 里的 `AbilityAttack.Init()`
- 如果某节点行为是 `AbilityBehaviorHurt`，会按 `AttackType` 建立 `hurtNodeDict`。

### Tick 顺序
- 固定驱动仍然来自 `HaloFrame` 的 15 FPS 逻辑帧循环。
- `BehaviorComp.Tick(float deltaTime)` 当前顺序是：
  1. 如果实体死亡或当前节点为空，直接返回。
  2. 先尝试选出下一个节点。
  3. 如果有 `EffectComp`，允许 Buff 通过 `OnStartBehavior(...)` 改写目标节点。
  4. 执行当前节点 `Tick(curFrame)`。
  5. `curFrame += 1`。
  6. 超过 `FrameLength` 后进入循环或回到根节点。
- `EndBehavior()` 会切回 `Id == 0` 的根节点，并清空 `Entity.Target`。

## 当前跳转模型
- `AbilityNode.Childs` 保存的是“子节点 Id”，不是数组下标。
- `TryGetNextBehavior()` 的真实判定条件是：
  - 当前节点 `curNode.CanCancel == true`
  - 输入缓存 `GameManager_Input.Instance.bufferKeys` 中包含目标行为的 `InputKey`
  - 在满足输入的子节点中选择 `Priority` 最高者
- `AbilityNode.conditions` 依然存在，`AbilityNode.CheckCondition()` 也还在，但 **当前 `BehaviorComp.TryGetNextBehavior()` 并没有调用它**。
- `CancelAction` 会在动作窗口里把 `tree.curNode.CanCancel` 置为 `true`，因此“能否取消”现在更像是当前节点自身的状态，而不是对子节点条件的统一检查。

## 行为、动作与攻击窗口
- `BehaviorBase` 负责动作窗口：
  - `curFrame == StartFrame` 时 `AbilityAction.Enter(tree)`
  - `StartFrame <= curFrame <= EndFrame` 时 `AbilityAction.Tick(curFrame)`
  - `curFrame == EndFrame` 时 `AbilityAction.Exit()`
- `AbilityBehaviorAttack.Tick()` 在基础动作之外还会驱动 `AbilityAttack`：
  - 命中窗口开始时调用 `attack.Enter(entity)`
  - 窗口中执行 `attack.Tick(curFrame)`
  - 窗口结束时调用 `attack.Exit()`
- `AbilityBehaviorHurt` 主要承担受击节点分类，`BehaviorComp.GetHurtBehavior(AttackType)` 会从 `hurtNodeDict` 里取对应节点。

## 连招编辑器

### 入口
- 文件：`Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs`
- 菜单：`Tools/Ability/Combo Editor`
- 编辑器直接打开 `ActorComboGraphSO`，不再通过 `ActorData` 间接打开。

### 文档模型
- `ComboEditorDocument.Load(...)` 先调用 `ComboGraphNodeSubAssetUtility.EnsureNodesAreEmbedded(...)`。
- 文档内部维护：
  - 节点列表
  - 节点连线
  - 位置缓存
  - 本次编辑会话里创建或识别到的“本地行为”集合
  - 待删除节点集合

### 编辑器主要组件
- `ComboEditorWindow`：窗口、工具栏、图加载、保存、校验、自动布局、创建节点。
- `ComboGraphView`：GraphView 画布、连线、右键建点、运行时高亮。
- `ComboNodeView`：单节点卡片，可直接编辑优先级、条件、绑定行为。
- `ComboInspectorPane`：右侧双标签页，分别查看/编辑 Behavior 与 Node。
- `ComboNodeEditorUtility`：创建本地行为、添加条件、设置优先级、绑定行为。
- `ComboGraphSaveService`：把文档状态回写到资产。
- `ComboGraphValidation`：保存前完整性校验。

### 创建节点与行为
- 新节点通过 `AssetDatabase.AddObjectToAsset(node, document.ComboGraph)` 挂到当前图资产下，属于图的子资产。
- 新节点 Id 规则是 `max(existing Id) + 1`。
- “Create Local Behavior” 创建的是独立 `.asset` 文件，默认放在连招图所在目录，不是图的子资产。

### 运行时高亮
- Play Mode 下，`ComboEditorWindow` 会遍历 `FightManager.LogicEntity` 中所有 `Actor`。
- 只要角色的 `PlayerDataComp.Data.ComboGraph` 与当前编辑图一致，就会按 `BehaviorComp.curNode` 统计节点活跃数并高亮。

### 保存语义
- `ComboGraphSaveService.Apply(...)` 会：
  - 回写节点位置到 `AbilityNode.EditorPosition` / `EditorRect` / `HasEditorPosition`
  - 从图上的连线重建 `node.Childs`
  - 按 Id 排序重写 `comboGraph.Nodes`
- `Save(...)` 会：
  - 先 `Apply(...)`
  - 再做 `ComboGraphValidation.Validate(...)`
  - 标脏图、节点、绑定行为和本地行为
  - 真正删除 `removedNodes`
  - 保存资产
  - Play Mode 下热重载所有使用该图的角色运行时数据

### 校验规则
- 图不能为空
- 至少有一个节点
- 必须存在根节点 `Id == 0`
- 节点 Id 必须唯一
- 每个节点都必须绑定行为
- 每个 `Childs` 里的子节点 Id 都必须存在

## 高价值文件
| 任务 | 文件 |
|------|------|
| 连招运行时入口 | `Assets/Scripts/Ability/Actor/BehaviorComp.cs` |
| 角色选择连招图 | `Assets/Scripts/Ability/Actor/ActorData.cs` |
| 连招图资产 | `Assets/Scripts/Ability/Combo/ActorComboGraphSO.cs` |
| 节点数据 | `Assets/Scripts/Ability/AbilityNode.cs` |
| 行为窗口执行 | `Assets/Scripts/Ability/BehaviorBase.cs` |
| 攻击行为 | `Assets/Scripts/Ability/Behavior/AbilityBehaviorAttack.cs` |
| 编辑器窗口 | `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs` |
| 保存回写 | `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphSaveService.cs` |
| 节点嵌入工具 | `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphNodeSubAssetUtility.cs` |
| 图校验 | `Assets/Scripts/Ability/Combo/ComboGraphValidation.cs` |

## 当前易错点
- 旧版“运行时会从 `Resources` 扫描节点/行为”的认知已经过时，当前运行时只吃 `ActorData.ComboGraph`。
- `ActorComboGraphSO` 现在只有 `Nodes`；编辑器里的“本地行为集合”只是会话级跟踪和标脏辅助，不是图资产字段。
- 节点是图拥有的数据，建议作为图子资产维护；行为可以是外部共享资产，也可以是图目录下的本地资产。
- 编辑共享 `AbilityBehavior` 会影响所有引用它的节点，不只当前图。
- 节点在图上删除后，只有点击 `Save` 才会真正从资产里移除。
- 如果保存后节点又“回来”，先检查是否真的保存成功，或者是否被校验拦住。
- 迁移旧图时先用 `Tools/Ability/Embed Combo Graph Nodes`，把外部节点整理为图子资产。
