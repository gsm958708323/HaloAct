# Draft: Bullet System Improvements

## Requirements (confirmed)
- "完善子弹系统"

## Technical Decisions
- [TBD] Canonical bullet architecture: Actor-based bullet (reuses Ability/Attack pipeline) vs EntityType.Bullet-based bullet (needs render+collision).
- [TBD] Hit consequence: behavior-only (hurt animation/state) vs add HP/damage/death.
- [TBD] Collision authority: Unity trigger callbacks vs Tick-time sweep (SphereCast / logic-space).

## Research Findings
- Existing bullet scaffolding exists in both forms:
  - Actor-based bullet (id 3001) with prefab `Assets/Scripts/Ability/Res/Bullet.prefab` and behavior asset `Assets/Scripts/Ability/Resources/Ability/3001/Behavior/Default.asset`.
  - EntityType.Bullet-based bullet via `Assets/Scripts/Ability/Manager/EntityManager.cs:CreateBullet` + `Assets/Scripts/Ability/Manager/BulletManager.cs` but missing render/hit integration.
- Ability data already references bullet spawning:
  - `Assets/Scripts/Ability/Resources/Ability/1001/Behavior/Skill01.asset` contains `Ability.CreateBulletAction` with `bullet=3001`.
  - `Assets/Scripts/Ability/BuffAction/CreateBulletAction.cs` is present but currently commented out.
- Asset drift currently blocks bullet behavior/render correctness:
  - Bullet behavior `Assets/Scripts/Ability/Resources/Ability/3001/Behavior/Default.asset` references missing script GUID `300956a197b9faf459c85ea9c2b765d6`.
  - Bullet prefab `Assets/Scripts/Ability/Res/Bullet.prefab` references missing script GUID `013c58c763cd1d54a862aaee85cdbf6b` (fields `lineColor`, `lineWidth` suggest a collider visualizer; repo has `Assets/Scripts/Ability/CollisionBox/ColliderVisualizer.cs`).
- Combat today has no actor HP/damage system:
  - Melee hit pipeline triggers hurt behaviors via `AttackComp`/`BehaviorComp`, but `Entity.DeathCheck()` is empty and there is no actor HP component.

## Open Questions
- Which bullet architecture should we standardize on (Actor vs EntityType.Bullet)?
- Should bullets apply numeric damage/HP, or only trigger hurt state transitions?
- Which collision model should bullets use (triggers vs Tick-time sweeps)?

## Scope Boundaries
- INCLUDE: bullet spawn, movement, collision/hit, lifetime/removal, config usage, and integration with existing Ability/Attack pipeline.
- EXCLUDE (unless requested): full combat HP/death system, UI damage numbers, netcode determinism.
