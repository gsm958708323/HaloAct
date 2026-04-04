# Assets/Scripts/HaloFrame/Editor/Buidler / AGENTS

## 模块概览
这里是编辑器侧的 AssetBundle 打包与热更包构建工具。目录名 `Buidler` 是历史拼写，当前仍然是实际路径的一部分。

## 入口
- 编辑器窗口：`Assets/Scripts/HaloFrame/Editor/Buidler/BuildSettingsEditorWindow.cs`
  - 菜单：`Tools/HaloFrame/打包编辑器`
  - 热键：`F5`
- 构建主逻辑：`Assets/Scripts/HaloFrame/Editor/Buidler/Builder.cs`
- 构建配置资产：`Assets/Scripts/HaloFrame/Editor/Buidler/BuildSettingsSO.cs`
- 约定配置路径：`PathTools.BuildSettingPath`，也就是 `Assets/BuildSetting.asset`

## BuildSettingsSO
- 文件：`Assets/Scripts/HaloFrame/Editor/Buidler/BuildSettingsSO.cs`
- 主要字段：
  - `projectName`
  - `version`
  - `buildRoot`
  - `remoteAddress`
  - `openHotUpdate`
  - `enablePackage`
  - `items: List<BuildItem>`
- `BuildSettingsEditorWindow.BuildMenuTree()` 的行为：
  - 先尝试加载 `Assets/BuildSetting.asset`
  - 如果不存在，就即时创建一个新的 `BuildSettingsSO`
  - 然后调用 `setting.Init()`
- `Init()` 会：
  - 把 `buildRoot` 归一化为绝对路径
  - 检查 `Rule` / `Directory` 类型规则的目录存在性
  - 解析 `suffix` 字符串为 `suffixes`
  - 建立 `itemDic`

## BuildItem 规则
- 文件：`Assets/Scripts/HaloFrame/Editor/Buidler/BuildItem.cs`
- 关键字段：
  - `assetPath`：规则根路径
  - `resourceType`：`Direct` / `Dependency`
  - `bundleType`：`File` / `Directory` / `Rule`
  - `suffix`：后缀过滤，使用 `|` 分隔
- `BuildSettingsSO.Collect()` 会对 Direct 规则构建 `ignorePaths`，避免父目录规则把子目录规则重复打包。

## 构建流程

### 1. 读取配置
- `Builder.LoadSettingSO(...)` 读取配置资产，并生成：
  - `buildPath = <buildRoot>/<Platform>`
  - `hotUpdateBuildPath = <buildPath>/HotUpdate_<version>/`

### 2. 收集直接资源
- `BuildSettingsSO.Collect()` 会先按 Direct 规则收集文件，再过滤被更细规则覆盖的路径。

### 3. 收集依赖
- `Builder.CollectDependency(...)` 调用 `AssetDatabase.GetDependencies(...)`
- 过滤 `.cs`、`.dll` 和没有扩展名的项
- 结果存成 `Dictionary<string, List<string>> dependencyDic`

### 4. 计算 bundle 划分
- `Builder.CollectBundleSO(...)` 会根据 `BuildItem.bundleType` 决定 bundle 名：
  - `Rule`：整条规则路径打成一个 bundle
  - `Directory`：按资源所在目录打 bundle
  - `File`：单文件单 bundle
- 如果资源不匹配任何规则，会直接抛异常

### 5. 生成版本与资源表
- `Builder.GenerateResMap(...)` 会写出：
  - `<buildPath>/GameVersion.json`
  - `<buildPath>/AssetMap.json`
- 全量构建时还会额外写出：
  - `Assets/Resources/GameVersion.json`
  - `Assets/Resources/AssetMap.json`
- 全量构建写到 `Resources` 的 `AssetMap.json` 会把每项 `Md5` 清空，保证首次进入时走完整热更比对链路。

### 6. 构建 AssetBundle
- `Builder.BuildBundle(...)` 调用 `BuildPipeline.BuildAssetBundles(...)`
- 注释里已经明确写了：传入路径必须以 `/` 结尾，否则不会生成 `.ab` 文件

### 7. 清理输出
- `Builder.ClearAssetBundle(...)` 会：
  - 先把 Unity 默认生成的主清单重命名为 `main` / `main.manifest`
  - 再扫描输出目录
  - 并行删除所有不在当前 bundle 集合里的文件

## 命令入口
```text
Unity.exe -batchmode -nographics -quit -projectPath <repo> -executeMethod HaloFrame.Builder.Build
Unity.exe -batchmode -nographics -quit -projectPath <repo> -executeMethod HaloFrame.Builder.BuildUpdate
```

## 当前易错点
- `Assets/BuildSetting.asset` 目前是按需创建的，不要假设仓库里一定已经存在它。
- `hotUpdateBuildPath` 必须保留尾部 `/`，这是当前代码注释明确依赖的行为。
- `BuildUpdate()` 要求 `<buildPath>/AssetMap.json` 先存在；如果从未做过全量构建，增量热更包会直接报错退出。
- `ClearAssetBundle(...)` 是并行删除，`buildRoot` 绝对不能指向包含其他重要文件的目录。
- `Builder.RenameMainFile(...)` 会把 Unity 自动生成的主清单重命名为 `main`，相关运行时路径都是按这个名字读取的。
