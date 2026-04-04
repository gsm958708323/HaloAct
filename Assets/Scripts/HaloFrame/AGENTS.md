# Assets/Scripts/HaloFrame / AGENTS

## 模块概览
`HaloFrame` 是项目内自研框架层，提供这些基础能力：

- 管理器循环：区分 `Update()` 和固定步长 `Tick()`
- 驱动系统：让非 `MonoBehaviour` 对象拥有逐帧更新能力
- 事件系统：安全增删监听
- 下载、资源、AssetBundle、热更
- 编辑器打包与热更构建工具

## 核心循环
- 入口文件：`Assets/Scripts/HaloFrame/Runtime/Manager/GameManagerBase.cs`
- 关键行为：
  - `TargetFrameRate = 15`
  - 每个 Unity 帧先调用所有管理器的 `Update(deltaTime)`
  - 再按 `FrameInterval = 1 / TargetFrameRate` 补跑 `Tick(FrameInterval)`
  - `CurFrame` 每完成一次逻辑帧循环就递增
- 管理器 API：`Assets/Scripts/HaloFrame/Runtime/Manager/IManager.cs`
  - 生命周期：`Init -> Enter -> Update/Tick -> Exit -> Destroy`
  - 默认优先级 `0`
- `GetManager<T>()` 会按 `Priority` 插入链表，优先级高的先更新、销毁时后销毁。

## 关键子系统
| 子系统 | 文件 |
|--------|------|
| 管理器循环 | `Assets/Scripts/HaloFrame/Runtime/Manager/GameManagerBase.cs` |
| 驱动系统 | `Assets/Scripts/HaloFrame/Runtime/Manager/DriverManager.cs` |
| 事件系统 | `Assets/Scripts/HaloFrame/Runtime/Event/DispatcherBase.cs` + `Dispatcher.cs` |
| 下载系统 | `Assets/Scripts/HaloFrame/Runtime/Download/DownloadManager.cs` |
| 资源加载 | `Assets/Scripts/HaloFrame/Runtime/Res/ResourceManager.cs` |
| AssetBundle 管理 | `Assets/Scripts/HaloFrame/Runtime/Res/BundleManager.cs` |
| 热更 | `Assets/Scripts/HaloFrame/Runtime/Res/HotUpdateManger.cs` |
| 路径工具 | `Assets/Scripts/HaloFrame/Runtime/Tools/PathTools.cs` |
| 打包工具 | `Assets/Scripts/HaloFrame/Editor/Buidler/` |

## 约定
- 固定玩法逻辑应写在 `Tick()`，渲染或非确定性逻辑应写在 `Update()`。
- 资源、热更、打包路径统一优先使用 `PathTools.Combine(...)`，避免反斜杠问题。
- 事件系统允许在派发期间移除监听器，实际删除会延迟到派发结束后完成。
- `HaloFrame` 目录下存在一些“像底层框架”的代码，但当前仍直接引用全局 `Debugger` / `LogDomain`。

## 当前易错点
- 拼写错误属于现有公共表面：`HotUpdateManger`、`Buidler` 这些名字不要轻易修改。
- `ResourceManager` 和 `HotUpdateManger` 没有在 `GameManager` 默认启动链路里自动接上；要启用它们，需要补完整初始化流程。
- `DownloadManager.OnInit(...)` 不是 `IManager.Init()` 的重写，只有手动调用或另行注入 helper 后，文件大小查询相关 API 才是可用状态。
- `PathTools` 会缓存多个静态路径前缀；如果运行中热更地址或版本号发生变化，之前已经缓存的路径不会自动重算。
- `Assets/Scripts/HaloFrame/Plugins/LitJson` 是第三方代码，修改它等同于修改 vendor。
