# Assets/Scripts/Ability/Manager / AGENTS

## 模块概览
这个目录负责把战斗运行时串起来：场景引导、配置加载、逻辑实体、表现实体、子弹管理、输入、相机和调试输出都在这里。

## 场景启动链路
- 主测试场景：`Assets/Scenes/AbilityTest.unity`
- 核心入口组件：
  - `Assets/Main.cs`
  - `Assets/Scripts/Ability/Manager/GameManager.cs`
  - `Assets/Scripts/Ability/Manager/FightManager.cs`
- 脚本执行顺序：
  - `GameManager`：`-20`
  - `FightManager`：`-10`
- `Main.Start()` 当前会：
  - 通过 `FightManager.LogicEntity.CreateActor(1001)` 创建玩家
  - 调用 `cameraMgr.Bind(player.Uid)` 绑定相机
  - 再创建 `2001` 号怪物

## 关键管理器
| 管理器 | 文件 | 作用 |
|--------|------|------|
| `GameManager` | `Assets/Scripts/Ability/Manager/GameManager.cs` | 全局框架入口，暴露 `Dispatcher`、`DriverManager`、`RedDot`、`Download` |
| `FightManager` | `Assets/Scripts/Ability/Manager/FightManager.cs` | 战斗入口，创建 `Config`、`LogicEntity`、`RenderEntity`、`Bullet`、`GameInput` |
| `ConfigManager` | `Assets/Scripts/Ability/Manager/ConfigManager.cs` | 通过 `Resources.Load` 加载 `Actor` / `Buff` / `Bullet` 配置 |
| `EntityManager` | `Assets/Scripts/Ability/Manager/EntityManager.cs` | 创建逻辑实体、挂逻辑组件、派发创建/删除事件 |
| `EntityRenderManager` | `Assets/Scripts/Ability/Manager/EntityRenderManager.cs` | 监听实体事件，实例化 Prefab，驱动表现层实体 |
| `BulletManager` | `Assets/Scripts/Ability/Manager/BulletManager.cs` | 处理子弹生命周期、移动、碰撞、命中回调 |
| `PlayerGameInput` | `Assets/Scripts/Ability/Manager/PlayerGameInput.cs` | 新 Input System 包装器 |
| `Debugger` | `Assets/Scripts/Ability/Manager/Debugger.cs` | 全局日志过滤与富文本输出 |

## 实体与组件链路
- 公共实体容器：`Assets/Scripts/Ability/Manager/IEntityManager.cs`
  - 维护 `entityDict`
  - 维护按 `EntityType` 分组的 `LinkedList<IEntity>`
  - `DriveEntity(deltaTime)` 逐个调用 `entity.Tick(...)`
- 角色创建链路：
  1. `EntityManager.CreateActor(int cfgId)` 从 `ConfigManager` 读取 `ActorData`
  2. 创建逻辑实体并挂：
     - `PlayerDataComp`
     - `TransfromComp`
     - `BehaviorComp`
     - `EffectComp`
     - `AttackComp`
  3. 通过 `GameManager.Dispatcher.Notify<Entity>(EventId.CreateEntity, entity)` 派发创建事件
  4. `EntityRenderManager.OnCreateEntity(...)` 实例化 `ActorData.Prefab`
  5. 创建 `EntityRender`，绑定 GameObject，并挂 `RenderTransformComp`
- 子弹创建链路与角色类似，但逻辑组件是 `BulletDataComp`，表现侧挂的是 `BulletRenderTransformComp`。

## 子弹逻辑
- 文件：`Assets/Scripts/Ability/Manager/BulletManager.cs`
- 子弹在 `Tick()` 中处理：
  - 持续时间
  - 剩余命中次数
  - 当前位置推进
  - `Physics.SphereCastAll(...)` 碰撞检测
- 命中角色的判定依赖：
  - `HurtBox`
  - `IdentitCard`
  - 阵营判定（`ActorType`）
  - `BulletDataComp.CanHitTarget(...)`
- 如果命中障碍并且 `RemoveOnObstacle == true`，子弹会直接删除。

## 输入与相机
- `PlayerGameInput` 使用新 Input System，创建并启用 `GameInput`，供相机等系统读取。
- `CameraMgr` 会从 `FightManager.GameInput.GetPlayerInput().CameraLook` 读取镜头输入。
- 连招输入不是从新 Input System 来的，而是来自 `Assets/Scripts/Ability/GameManager_Input.cs`：
  - 每帧遍历所有 `KeyCode`
  - 维护 `bufferKeys`
  - `BehaviorComp` 用它判断连招切换

## 调试输出
- `Debugger.logDict` 是当前启用的日志域集合。
- 输出使用富文本颜色标签。
- 这个 `Debugger` 虽然在 Ability 目录里，但 `HaloFrame` 运行时也会直接使用它。

## 当前易错点
- `GameManager` 当前只初始化了 `Dispatcher`、`DriverManager`、`RedDot`、`Download`；`Resource` 和 `UI` 仍然注释掉。
- 如果重新启用 `ResourceManager`，不能只把注释打开，因为 `ResourceManager` 需要显式 `Init(bundleRootDir, ...)`，而不是默认 `IManager.Init()`。
- `FightManager.Config`、`FightManager.LogicEntity`、`FightManager.RenderEntity` 等静态字段只有在 `FightManager.InitManager()` 之后才有效。
- `EntityRenderManager` 依赖 `GameManager.Dispatcher` 注册事件，场景里如果缺少 `GameManager`，逻辑实体不会自动生成表现层对象。
- `GameManager_Input` 每帧扫描全部 `KeyCode`，是明显偏旧的输入实现；修改连招输入前要先确认是否要与 `PlayerGameInput` 合并。
