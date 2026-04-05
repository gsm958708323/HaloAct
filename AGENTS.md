# HaloAct / AGENTS

更新日期：2026-04-04
Unity：2021.3.37f1（`ProjectSettings/ProjectVersion.txt`）
分支：`master`
提交：`44ceecc`

## 项目概览
HaloAct 是一个基于固定逻辑帧的动作/连招原型项目。玩法层代码集中在 `Assets/Scripts/Ability/`，底层框架、资源、下载与打包链路集中在 `Assets/Scripts/HaloFrame/`。

当前连招数据模型已经收敛到：

`ActorData.ComboGraph -> ActorComboGraphSO -> AbilityNode -> AbilityBehavior -> AbilityAction / AbilityAttack`

同时项目内已经有新的连招可视化编辑器，入口位于 `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs`，直接编辑 `ActorComboGraphSO` 资产。

## 目录速览
```text
./
|- Assets/
|  |- Main.cs                              # 场景启动脚本，开局生成玩家/怪物并绑定相机
|  |- Scenes/AbilityTest.unity             # 当前主测试场景
|  |- Res/Input/GameInput.*                # 新 Input System 输入资产与生成代码
|  |- Scripts/
|  |  |- Ability/                          # 战斗、连招、实体、子弹、编辑器
|  |  \- HaloFrame/                        # 管理器循环、事件、资源、热更、打包工具
|  \- Plugins/                             # 第三方插件（Sirenix / ParadoxNotion）
|- Packages/manifest.json                  # UPM 依赖
|- ProjectSettings/                        # Unity 工程配置
|- Doc/                                    # 旧说明、截图等静态文档
\- docs/                                   # 新文档与计划
```

## 建议先读的入口
| 任务 | 入口文件 | 说明 |
|------|----------|------|
| 场景如何启动 | `Assets/Main.cs` | `Start()` 会生成 `1001` 和 `2001` 两个角色，并把相机绑定到玩家 |
| 固定帧逻辑怎么跑 | `Assets/Scripts/HaloFrame/Runtime/Manager/GameManagerBase.cs` | `Update()` 负责渲染帧更新，`Tick()` 以 15 FPS 驱动逻辑 |
| 连招运行时怎么切换节点 | `Assets/Scripts/Ability/Actor/BehaviorComp.cs` | 负责装载图、切换节点、推进帧数、处理循环/结束 |
| 连招图资产结构 | `Assets/Scripts/Ability/Combo/ActorComboGraphSO.cs` | 图本体只保存 `Nodes`，不再保存旧版 `LocalBehaviors` |
| 连招编辑器 | `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs` | 直接编辑 `ActorComboGraphSO`，支持运行时高亮 |
| 逻辑实体如何生成表现 | `Assets/Scripts/Ability/Manager/EntityManager.cs` + `Assets/Scripts/Ability/Manager/EntityRenderManager.cs` | 逻辑实体创建后通过事件通知表现层实例化 Prefab |
| 子弹与碰撞 | `Assets/Scripts/Ability/Manager/BulletManager.cs` | 子弹存活、碰撞、友伤/敌伤、障碍销毁都在这里 |
| 资源与热更 | `Assets/Scripts/HaloFrame/Runtime/Res/ResourceManager.cs` + `Assets/Scripts/HaloFrame/Runtime/Res/HotUpdateManger.cs` | 资源加载依赖 `AssetMap.json`，热更负责比较 MD5 并下载 |
| AssetBundle 打包 | `Assets/Scripts/HaloFrame/Editor/Buidler/Builder.cs` | 生成 `GameVersion.json`、`AssetMap.json` 并构建 AB |

## 项目约定
- 逻辑帧默认是 15 FPS，战斗玩法优先写在 `Tick()`，渲染或输入优先写在 `Update()`。
- `GameManager` 的脚本执行顺序是 `-20`，`FightManager` 是 `-10`，场景启动依赖这个顺序。
- 路径拼接优先用 `PathTools.Combine(...)`，避免 Windows 反斜杠影响 Unity API。
- 工程启用了文本序列化（`ProjectSettings/EditorSettings.asset` 中 `m_SerializationMode: 2`），Meta 文件可见。
- VS Code 工作区显式隐藏了大量 Unity 生成目录，连 `ProjectSettings/` 也被隐藏，不代表这些目录不存在。

## 当前易错点
- 不要随意“修正”公开命名里的拼写问题：`HotUpdateManger`、`Buidler`、`StarHotUpdate` 都已经被代码和路径引用。
- `BehaviorComp.TryGetNextBehavior()` 当前只检查“当前节点是否可取消”和“输入缓存里是否有目标行为的按键”，不会调用 `AbilityNode.CheckCondition()`。
- 连招输入仍然来自 `GameManager_Input` 对全部 `KeyCode` 的轮询；相机输入走的是 `PlayerGameInput` 包装的新 Input System，两套输入链路并存。
- `ProjectSettings/EditorBuildSettings.asset` 仍然没有任何启用场景，打 Player 包时不能依赖默认 Build Settings。
- `Assets/BuildSetting.asset` 目前不在仓库里；打开 `Tools/HaloFrame/打包编辑器` 时，`BuildSettingsEditorWindow` 会按 `PathTools.BuildSettingPath` 自动创建它。
- `Builder.ClearAssetBundle(...)` 会并行删除输出目录中不属于当前 bundle 集合的文件，`buildRoot` 必须指向专用构建目录。

## 子模块文档
- `Assets/Scripts/AGENTS.md`
- `Assets/Scripts/Ability/AGENTS.md`
- `Assets/Scripts/Ability/Manager/AGENTS.md`
- `Assets/Scripts/HaloFrame/AGENTS.md`
- `Assets/Scripts/HaloFrame/Runtime/Res/AGENTS.md`
- `Assets/Scripts/HaloFrame/Editor/Buidler/AGENTS.md`
- `Assets/Plugins/AGENTS.md`

## Unity 测试约定
- 批量执行 EditMode 测试时，默认入口是 `tools/Run-HaloActEditModeTests.ps1`，不要在 batchmode 里走 `-executeMethod HaloFrame.Editor.HaloActEditModeBatchRunner.RunFromCommandLine`。
- 测试产物默认写到 `TestArtifacts/TestRunner`；不要把 `-testResults`、日志或摘要写到项目 `Temp/`，Unity 退出时可能清掉它们。
- 同一个工程的 Unity batchmode 测试必须串行执行；不要并行跑，也不要在 Unity Editor 打开工程时再启动另一条 batchmode 测试。
- 选测试时优先用脚本便捷参数：`-TestClass`、`-TestClass + -TestMethod`、`-TestNamespace`；只有需要更细控制时再用原始 `-TestFilter`。
- 当前项目的 EditMode 测试默认应放在 `Assets/Tests/EditMode/Editor/` 下，让它们编进 `Assembly-CSharp-Editor`。除非被测运行时代码也已经 asmdef 化，否则不要恢复独立 test asmdef。
