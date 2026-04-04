# Assets/Scripts/HaloFrame/Runtime/Res / AGENTS

## 模块概览
这个目录负责运行时资源链路，主要由三部分组成：

- `ResourceManager`：面向资源 URL 的引用计数、依赖加载与异步轮询。
- `BundleManager`：面向 AssetBundle 的加载、依赖解析与卸载。
- `HotUpdateManger`：读取本地/远端版本，比较 MD5，下载热更文件并回写到沙盒。

## 关键数据文件

### 构建产物
- `GameVersion.json`
  - 记录版本号和热更服务器地址
- `AssetMap.json`
  - 记录资源到 AB 的映射、依赖、MD5、大小、版本
- `main` / `main.manifest`
  - AssetBundle 主清单，`BundleManager` 依赖它解析 AB 依赖关系

### 运行时查找位置
- 首次运行回退：
  - `Resources.Load<TextAsset>("GameVersion")`
  - `Resources.Load<TextAsset>("AssetMap")`
- 沙盒覆盖：
  - `PathTools.LocalGameVersionPath`
  - `PathTools.LocalAssetMapPath`
- 热更下载目录：
  - `PathTools.DownloadABPathPrefix`

## ResourceManager
- 文件：`Assets/Scripts/HaloFrame/Runtime/Res/ResourceManager.cs`
- 关键点：
  - `Init(string bundleRootDir, bool isEditor = false, ulong offset = 0)` 会初始化字典、队列，并在非 Editor 模式下初始化 `BundleManager`
  - `Tick()` 中会依次驱动：
    - `BundleManager.Instance.CheckLoad()`
    - 资源异步完成检查
    - 延迟卸载检查
    - `BundleManager.Instance.CheckUnload()`
  - `LoadInternal(url, async)` 会：
    - 先检查资源缓存与引用计数
    - 再通过 `GameConfig.RemoteAssetMap[assetUrl].Dependency` 递归加载依赖资源
    - 最后触发真实加载
  - `Unload(...)` 只是减引用；真正释放会放入 `waitUnloadList` 延迟处理

## BundleManager
- 文件：`Assets/Scripts/HaloFrame/Runtime/Res/BundleManager.cs`
- 关键点：
  - `Init(bundleRootDir, offset)` 会从 `<bundleRootDir>/main` 读取主清单
  - 依赖解析使用 `manifestAB.GetDirectDependencies(url)`
  - `LoadInternal(url, async)` 会先递归加载依赖 AB，再加载目标 AB
  - 卸载采用引用计数，引用归零后加入 `waitUnloadList`，随后级联卸载依赖

## HotUpdateManger
- 文件：`Assets/Scripts/HaloFrame/Runtime/Res/HotUpdateManger.cs`

### 初始化阶段
- `Init()` 会通过 `GameManager.Download.CreateDownloader("HotUpdate")` 创建下载器，并绑定下载成功、失败、开始、整体进度、全部完成回调。

### 进入运行时阶段
- `Enter()` 会优先尝试从沙盒读取本地版本和资源表。
- 如果沙盒不存在，则回退到 `Resources` 中的 `GameVersion` / `AssetMap`。
- 读取完成后会写入：
  - `GameConfig.LocalVersion`
  - `GameConfig.HotUpdateAddress`
  - `GameConfig.LocalAssetMap`
  - `GameConfig.RemoteAssetMap`（初始默认与本地一致）

### 请求远端
- `ReqRemote()` 会依次请求：
  - `<HotUpdateAddress>/<Platform>/GameVersion.json`
  - `<HotUpdateAddress>/<Platform>/AssetMap.json`
- 如果版本相同，会直接结束，不继续请求资源表。

### 热更判定
- `CheckHotUpdate()` 会比较本地与远端 `AssetInfo.Md5`。
- 只要本地不存在、或 MD5 不同，就会加入热更集合，并累计下载大小。

### 启动下载
- `StarHotUpdate(Action finishCB)` 会把以下文件加入下载队列：
  - 需要更新的 AB
  - 主清单文件 `main`
- 下载完成后，`UpdateRemoteToLocal()` 会把远端版本与远端资源表回写到沙盒。

## 相关数据类型
| 类型 | 文件 | 说明 |
|------|------|------|
| `AssetInfo` | `Assets/Scripts/HaloFrame/Runtime/Res/HotUpdate/AssetInfo.cs` | 依赖列表、AB 路径、版本、MD5、大小 |
| `GameVersion` | `Assets/Scripts/HaloFrame/Runtime/Res/HotUpdate/GameVersion.cs` | 版本号 + 远端地址 |
| `GameConfig` | `Assets/Scripts/HaloFrame/Runtime/Res/HotUpdate/GameConfig.cs` | 运行时全局缓存：本地/远端版本与资源表 |
| `PathTools` | `Assets/Scripts/HaloFrame/Runtime/Tools/PathTools.cs` | 平台路径、沙盒路径、热更目录拼接 |

## 当前易错点
- `ResourceManager.Init(...)` 不是 `IManager.Init()` 的重写；如果只是 `GetManager<ResourceManager>()`，内部字典和 `BundleManager` 不会自动正确初始化。
- `ResourceManager.GetAssetInfo(url)` 直接读取 `GameConfig.RemoteAssetMap`，所以版本表/资源表必须先准备好。
- `HotUpdateManger.CompareVersion(...)` 在本地版本文件不存在时会强制走更新逻辑。
- `ReqRemote()` 目前没有重试、超时恢复或降级逻辑，网络失败会直接 `yield break`。
- `PathTools.RemoteABUrlPrefix`、`DownloadABPathPrefix`、`HotUpdateVersionDir` 都带静态缓存；一旦首次访问后，后续版本变化不会自动刷新这些字段。
