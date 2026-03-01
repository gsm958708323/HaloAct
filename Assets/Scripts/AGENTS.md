# Assets/Scripts / AGENTS

## Overview
First-party code under `Assets/Scripts/` is split into two modules:
- `Ability/`: gameplay (frame-driven ability/combo system)
- `HaloFrame/`: reusable runtime/editor framework (manager loop, events, res/bundles, build tools)

Dependency direction: `Ability` uses `HaloFrame`. Keep `HaloFrame` free of `Ability` references to avoid circular coupling.

## Structure
```
Assets/Scripts/
├── Ability/
│   ├── Actor/          # Entities + comps + BehaviorComp runner
│   ├── Action/         # Concrete AbilityAction implementations
│   ├── Behavior/        # AbilityBehavior ScriptableObjects (Root/Attack/Hurt)
│   ├── Condition/       # AbilityCondition implementations
│   └── Manager/         # GameManager/FightManager/EntityManagers
└── HaloFrame/
    ├── Runtime/         # Core loop + systems
    ├── Editor/          # Build UI + Builder.cs
    └── Plugins/LitJson/ # Vendored JSON (treat as third-party-in-repo)
```

## Where To Look
| Question | Start Here |
|----------|------------|
| “Why is this logic not updating?” | `Assets/Scripts/HaloFrame/Runtime/Manager/GameManagerBase.cs` (Update vs Tick) |
| “How do combos/inputs switch nodes?” | `Assets/Scripts/Ability/Actor/BehaviorComp.cs` + `Assets/Scripts/Ability/GameManager_Input.cs` |
| “Where is the entity created/rendered?” | `Assets/Scripts/Ability/Manager/EntityManager.cs` + `Assets/Scripts/Ability/Manager/EntityRenderManager.cs` |
| “How are bundles/version/map generated?” | `Assets/Scripts/HaloFrame/Editor/Buidler/Builder.cs` |

## Conventions
- Namespaces: most code uses `namespace Ability` or `namespace HaloFrame`.
- A few key boot scripts are in the global namespace (e.g., `GameManager`, `FightManager`, `ConfigManager`) for Unity component convenience.
- Prefer fixed-step gameplay logic in `Tick()` and presentation/input in `Update()` (matches `GameManagerBase`).

## Anti-Patterns
- Don’t move scripts across module boundaries casually; Unity component references are GUID-based, but assembly definitions and build rules may assume paths.
- Don’t “clean up” `HaloFrame/Plugins/LitJson` unless you are intentionally upgrading vendor code.
