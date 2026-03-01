# Assets/Scripts/Ability/Manager / AGENTS

## Overview
This folder wires the gameplay runtime together: config loading, entity lifecycle, rendering bridge, input, camera, and logging.

## Scene Bootstrap
`Assets/Scenes/AbilityTest.unity` references (GUID-based) these key components:
- `Assets/Main.cs` (guid `cb598bc2087f99b45aa68d973e0a95f2`)
- `Assets/Scripts/Ability/Manager/GameManager.cs` (executionOrder `-20`)
- `Assets/Scripts/Ability/Manager/FightManager.cs` (executionOrder `-10`)

`Assets/Main.cs` spawns actors via `FightManager.LogicEntity.CreateActor(...)` and binds the camera.

## Key Managers
| Manager | Location | Role |
|--------|----------|------|
| `GameManager` | `Assets/Scripts/Ability/Manager/GameManager.cs` | Global entry; exposes static `Dispatcher`, `DriverManager`, `RedDot`, `Download` |
| `FightManager` | `Assets/Scripts/Ability/Manager/FightManager.cs` | Ability-side entry; sets `Config`, `LogicEntity`, `RenderEntity`, `Bullet`, `GameInput` |
| `ConfigManager` | `Assets/Scripts/Ability/Manager/ConfigManager.cs` | `Resources.Load` wrappers (`Actor/<id>`, `Buff/<id>`, `Bullet/<id>`) |
| `EntityManager` | `Assets/Scripts/Ability/Manager/EntityManager.cs` | Creates logical entities and notifies `EventId.CreateEntity` |
| `EntityRenderManager` | `Assets/Scripts/Ability/Manager/EntityRenderManager.cs` | Listens `CreateEntity`, instantiates `ActorData.Prefab`, binds render comps |
| `BulletManager` | `Assets/Scripts/Ability/Manager/BulletManager.cs` | Ticks bullet entities (logic) |

## Entities + Components
- Entity store/driver: `Assets/Scripts/Ability/Manager/IEntityManager.cs` holds `entityDict` + `entityUidList` and calls `entity.Tick(...)`.
- Component model: `Assets/Scripts/Ability/Actor/IEntity.cs` (`AddComp/GetComp/RemoveComp`) and `Assets/Scripts/Ability/Actor/IComponent.cs` (`ComponentLogic`/`ComponentRender`).

## Input
- New Input System wrapper: `Assets/Scripts/Ability/Manager/PlayerGameInput.cs` creates/enables `GameInput`.
- Generated input assets: `Assets/Res/Input/GameInput.cs` + `Assets/Res/Input/GameInput.inputactions`.
- Combo buffer input: `Assets/Scripts/Ability/GameManager_Input.cs` fills `bufferKeys` consumed by `BehaviorComp`.

## Logging
`Assets/Scripts/Ability/Manager/Debugger.cs`:
- `Debugger.Log*` is domain-filtered via `Debugger.logDict` (Odin dropdown) and prints colored rich-text tags.

## Gotchas
- `GameManager` has `Resource` and `UI` fields but currently commented out in `InitManager()`; enabling them changes runtime expectations.
- Static manager fields (`FightManager.Config`, `FightManager.LogicEntity`, etc.) are set during `FightManager.InitManager()`; avoid accessing them earlier.
