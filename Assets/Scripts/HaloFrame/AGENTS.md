# Assets/Scripts/HaloFrame / AGENTS

## Overview
HaloFrame is the in-house framework providing:
- A manager loop with split `Update()` (render frame) vs `Tick()` (fixed step)
- Driver system for non-MonoBehaviour update
- Event dispatcher
- Resource/AssetBundle loading + hot update
- Editor build pipeline for bundles + version/map

## Core Loop
- Entry: `Assets/Scripts/HaloFrame/Runtime/Manager/GameManagerBase.cs`
  - `TargetFrameRate = 15`
  - Calls `IManager.Update(deltaTime)` each Unity frame
  - Accumulates time and calls `IManager.Tick(FrameInterval)` in a fixed-step loop
- Manager API: `Assets/Scripts/HaloFrame/Runtime/Manager/IManager.cs`
  - Lifecycle: `Init -> Enter -> Update/Tick -> Exit -> Destroy`
  - Priority: `GetManager<T>()` inserts by `IManager.Priority` (higher first)

## Key Subsystems
| Subsystem | Location |
|----------|----------|
| Drivers | `Assets/Scripts/HaloFrame/Runtime/Manager/DriverManager.cs` |
| Events | `Assets/Scripts/HaloFrame/Runtime/Event/DispatcherBase.cs` + `Assets/Scripts/HaloFrame/Runtime/Event/Dispatcher.cs` |
| Res/AB | `Assets/Scripts/HaloFrame/Runtime/Res/ResourceManager.cs` + `Assets/Scripts/HaloFrame/Runtime/Res/BundleManager.cs` |
| Hot update | `Assets/Scripts/HaloFrame/Runtime/Res/HotUpdateManger.cs` |
| Download | `Assets/Scripts/HaloFrame/Runtime/Download/DownloadManager.cs` |
| Paths | `Assets/Scripts/HaloFrame/Runtime/Tools/PathTools.cs` |
| Editor build | `Assets/Scripts/HaloFrame/Editor/Buidler/` |

## Conventions
- Path separator: Unity APIs are happier with `/`; `PathTools.Combine` normalizes `\\` to `/`.
- Events: `DispatcherBase` prevents unsafe mutation during dispatch via `processSet` + delayed delete.

## Gotchas
- Naming typos are part of public surface area: `HotUpdateManger`, `Buidler`.
- `ResourceManager.GetAssetInfo(url)` depends on `GameConfig.RemoteAssetMap`; ensure version/map is loaded before resource loads.
