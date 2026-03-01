# HaloAct / AGENTS

Generated: 2026-03-01T13:20:19+08:00
Unity: 2021.3.37f1 (`ProjectSettings/ProjectVersion.txt`)
Branch: master
Commit: f31725a

## Overview
HaloAct is a Unity project centered on a frame-driven ability/combo system ("AbilityBehaviorTree") under `Assets/Scripts/Ability/`, built on an in-house runtime framework `Assets/Scripts/HaloFrame/`.

## Structure
```
./
├── Assets/
│   ├── Main.cs                     # Scene bootstrap MonoBehaviour (spawns actors)
│   ├── Scenes/                     # e.g. AbilityTest.unity
│   ├── Res/                        # Art + InputSystem assets (GameInput.*)
│   ├── Scripts/
│   │   ├── Ability/                # AbilityBehaviorTree gameplay
│   │   └── HaloFrame/              # Manager loop, events, res/bundles, hotupdate, editor build
│   └── Plugins/                    # Third-party (Sirenix/Odin, ParadoxNotion/NodeCanvas)
├── Packages/manifest.json          # UPM deps
└── ProjectSettings/                # Unity project config
```

## Where To Look
| Task | Location | Notes |
|------|----------|-------|
| Open the main test scene | `Assets/Scenes/AbilityTest.unity` | References `Main`, `GameManager`, `FightManager` scripts via GUIDs |
| Gameplay bootstrap | `Assets/Main.cs` | `Start()` spawns actors `1001`/`2001`, binds `CameraMgr` |
| Fixed-step tick loop | `Assets/Scripts/HaloFrame/Runtime/Manager/GameManagerBase.cs` | `TargetFrameRate = 15`; drives managers via `Update()` + `Tick()` |
| Manager lifecycle contract | `Assets/Scripts/HaloFrame/Runtime/Manager/IManager.cs` | Base class with `Init/Enter/Update/Tick/Exit/Destroy` |
| Ability tree runtime | `Assets/Scripts/Ability/Actor/BehaviorComp.cs` | Loads nodes/behaviors from Resources and advances by `curFrame` |
| Ability nodes + transitions | `Assets/Scripts/Ability/AbilityNode.cs` | `Childs` + `conditions` + `Priority` + `Behavior` |
| Action frame windows | `Assets/Scripts/Ability/BehaviorBase.cs` | `StartFrame/EndFrame` gate `AbilityAction` Enter/Tick/Exit |
| Event bus | `Assets/Scripts/HaloFrame/Runtime/Event/DispatcherBase.cs` | Safe add/remove during dispatch (delayed delete) |
| Entity create -> render bind | `Assets/Scripts/Ability/Manager/EntityManager.cs` + `Assets/Scripts/Ability/Manager/EntityRenderManager.cs` | Notify `EventId.CreateEntity` -> instantiate `ActorData.Prefab` |
| Hot update runtime | `Assets/Scripts/HaloFrame/Runtime/Res/HotUpdateManger.cs` | Loads local version/map; diffs; downloads; writes to sandbox |
| AssetBundle build pipeline | `Assets/Scripts/HaloFrame/Editor/Buidler/Builder.cs` | Editor-only; generates `GameVersion.json` + `AssetMap.json` and builds bundles |
| Build settings asset | `Assets/BuildSetting.asset` | Created/edited by build window; path is `PathTools.BuildSettingPath` |

## Conventions (Repo-Specific)
- Serialization: text mode (`ProjectSettings/EditorSettings.asset` `m_SerializationMode: 2`) and meta files are visible (`ProjectSettings/VersionControlSettings.asset`).
- Tick model: most gameplay is written assuming a fixed-step `Tick()` loop at 15 FPS (`GameManagerBase.TargetFrameRate`).
- Script execution order: `GameManager` runs before `FightManager` (`Assets/Scripts/Ability/Manager/GameManager.cs.meta`, `Assets/Scripts/Ability/Manager/FightManager.cs.meta`).
- Paths: prefer `PathTools.Combine(...)` to normalize path separators (`Assets/Scripts/HaloFrame/Runtime/Tools/PathTools.cs`).
- IDE noise: `.editorconfig` disables `IDE0051` and `IDE0044` for `*.cs`.
- VS Code: workspace hides many Unity/generated folders and even `ProjectSettings/` (`.vscode/settings.json`).

## Anti-Patterns / Gotchas
- Avoid “typo-fixing” public names/paths without a full sweep: `HotUpdateManger`, `StarHotUpdate`, `Editor/Buidler` are referenced by code and/or asset paths.
- `ProjectSettings/EditorBuildSettings.asset` currently has no enabled scenes; player builds need explicit scene configuration or custom build tooling.
- `Builder.ClearAssetBundle(...)` deletes files not in the current bundle set (parallel `File.Delete`); ensure `BuildSettingsSO.buildRoot` points to a safe output directory.
- `GameManager_Input` scans all `KeyCode` values in `Update()` to fill `bufferKeys`; `BehaviorComp` uses this buffer for combo transitions.
- Third-party code under `Assets/Plugins/` includes its own deprecated APIs and editor workarounds; avoid patching vendor code unless upgrading.

## Commands / Workflows
```text
Unity Editor (recommended)
  - Open project with Unity 2021.3.37f1
  - AssetBundle build UI: Tools/HaloFrame/打包编辑器 (F5)
    - Config asset: Assets/BuildSetting.asset
    - Buttons: Build (full) / 构建热更包 (incremental)

Batchmode (template)
  Unity.exe -batchmode -nographics -quit -projectPath <repo> -executeMethod HaloFrame.Builder.Build
  Unity.exe -batchmode -nographics -quit -projectPath <repo> -executeMethod HaloFrame.Builder.BuildUpdate
```

## Sub-Agents (Deeper Docs)
- `Assets/Scripts/AGENTS.md`
- `Assets/Scripts/Ability/AGENTS.md`
- `Assets/Scripts/Ability/Manager/AGENTS.md`
- `Assets/Scripts/HaloFrame/AGENTS.md`
- `Assets/Scripts/HaloFrame/Runtime/Res/AGENTS.md`
- `Assets/Scripts/HaloFrame/Editor/Buidler/AGENTS.md`
- `Assets/Plugins/AGENTS.md`
