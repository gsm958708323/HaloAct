# Assets/Scripts/HaloFrame/Editor/Buidler / AGENTS

## Overview
Editor-only AssetBundle build + hot-update package generation.

Entry points:
- Window: `Tools/HaloFrame/打包编辑器` (hotkey `_F5`) in `Assets/Scripts/HaloFrame/Editor/Buidler/BuildSettingsEditorWindow.cs`
- Builder: `Assets/Scripts/HaloFrame/Editor/Buidler/Builder.cs`
- Settings asset: `Assets/BuildSetting.asset` (`PathTools.BuildSettingPath`)

## BuildSettingsSO
`Assets/Scripts/HaloFrame/Editor/Buidler/BuildSettingsSO.cs`:
- Core fields: `version`, `buildRoot`, `remoteAddress`, `openHotUpdate`, `enablePackage`.
- `items: List<BuildItem>` controls what assets are included.
- `Init()` normalizes `buildRoot` to an absolute path and parses `BuildItem.suffix` into `suffixes`.

## BuildItem Rules
`Assets/Scripts/HaloFrame/Editor/Buidler/BuildItem.cs`:
- `assetPath`: folder path
- `resourceType`: `Direct` or `Dependency` (`EResourceType`)
- `bundleType`: `File` / `Directory` / `Rule` (`EBundleType`)
- `suffix`: extensions joined by `|` (e.g. `.prefab|.png`)

Nested Direct rules are handled by `BuildSettingsSO.Collect()` via `ignorePaths` to avoid duplicate packing.

## Builder Outputs
`Assets/Scripts/HaloFrame/Editor/Buidler/Builder.cs`:
- `buildPath = <buildRoot>/<Platform>`
- `hotUpdateBuildPath = <buildPath>/HotUpdate_<version>/` (note trailing `/`)
- Writes:
  - `<buildPath>/GameVersion.json`
  - `<buildPath>/AssetMap.json`
  - `Assets/Resources/GameVersion.json` and `Assets/Resources/AssetMap.json` on full build
- Builds AssetBundles via `BuildPipeline.BuildAssetBundles(hotUpdateBuildPath, ...)`.

## Safety Notes / Gotchas
- `BuildPipeline.BuildAssetBundles` path must end with `/` (code comment). Don’t remove the trailing `/` from `hotUpdateBuildPath`.
- `ClearAssetBundle(...)` deletes extra files in the output directory in parallel; keep `buildRoot` away from any non-build folders.
- `BuildUpdate()` requires an existing `<buildPath>/AssetMap.json`; otherwise it logs an error and exits.
