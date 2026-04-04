# Assets/Scripts / AGENTS

## 模块概览
`Assets/Scripts/` 里的自研代码主要分成两层：

- `Ability/`：玩法与战斗层，负责角色、连招、Buff、子弹、编辑器。
- `HaloFrame/`：底层框架层，负责管理器循环、驱动、事件、资源、下载、打包。

理想依赖方向仍然是 `Ability -> HaloFrame`。但当前代码里 `HaloFrame` 也直接调用了全局 `Debugger` / `LogDomain`，因此它并不是完全独立的纯底层模块。

## 目录速览
```text
Assets/Scripts/
|- Ability/
|  |- Action/                            # 连招动作实现（位移、朝向、取消、发射子弹等）
|  |- Actor/                             # 逻辑实体、组件、BehaviorComp、Buff/Effect
|  |- Behavior/                          # AbilityBehavior 派生类（Normal / Attack / Hurt）
|  |- Combo/                             # 连招图资产、校验
|  |- Condition/                         # AbilityCondition 派生类
|  |- Editor/ComboEditor/                # 新连招图编辑器
|  \- Manager/                           # 场景级管理器、实体/渲染桥接、日志、输入
\- HaloFrame/
   |- Runtime/                           # 管理器循环、事件、资源、下载、工具
   |- Editor/Buidler/                    # AssetBundle 打包与热更构建
   \- Plugins/LitJson/                   # 仓库内第三方 JSON 代码
```

## 快速定位
| 问题 | 建议起点 |
|------|----------|
| 为什么逻辑没推进 | `Assets/Scripts/HaloFrame/Runtime/Manager/GameManagerBase.cs` |
| 连招为什么没跳节点 | `Assets/Scripts/Ability/Actor/BehaviorComp.cs` + `Assets/Scripts/Ability/GameManager_Input.cs` |
| 图编辑器保存后为什么没生效 | `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphSaveService.cs` |
| 角色是怎么创建并显示出来的 | `Assets/Scripts/Ability/Manager/EntityManager.cs` + `Assets/Scripts/Ability/Manager/EntityRenderManager.cs` |
| 子弹为什么没命中或没销毁 | `Assets/Scripts/Ability/Manager/BulletManager.cs` |
| 资源表和热更文件怎么生成 | `Assets/Scripts/HaloFrame/Editor/Buidler/Builder.cs` |
| 运行时资源怎么映射到 AB | `Assets/Scripts/HaloFrame/Runtime/Res/ResourceManager.cs` |

## 约定
- `Tick()` 处理固定步长玩法逻辑，`Update()` 处理渲染帧逻辑。
- `GameManagerBase.GetManager<T>()` 会按 `IManager.Priority` 把管理器插入链表，优先级高的先更新。
- `GameManager`、`FightManager`、`ConfigManager` 等场景引导类放在全局命名空间，属于 Unity 组件使用上的刻意选择。
- Ability 侧大量使用 `ScriptableObject` 作为配置与数据资产，Unity 引用链和 asset 路径都要慎改。

## 当前易错点
- `HaloFrame` 运行时并不完全“无玩法依赖”，日志输出直接依赖 `Assets/Scripts/Ability/Manager/Debugger.cs` 里的全局类型。
- `GameManager` 里 `Resource` 和 `UI` 仍处于注释状态；如果重新启用，不能只解除注释，还要补齐 `ResourceManager` 的显式初始化参数。
- `Ability` 目录里的 `Editor/ComboEditor` 是当前活跃的编辑器代码，旧的 `Resources.LoadAll` 心智模型已经不适用了。
- `HaloFrame/Plugins/LitJson` 虽然位于 `Assets/Scripts/` 下，但应当按第三方代码对待，不要顺手重构。
