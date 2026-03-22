# Bullet System Design (HaloAct)

Date: 2026-03-01
Owner: OpenCode
Status: Approved

## 1. Background

Current bullet-related code is incomplete and disconnected across logic/render layers:

- `BulletLaunchAction` validates frame settings but does not spawn bullets.
- `EntityManager.CreateBullet(...)` only creates logic entity + `BulletDataComp`; no spawn transform/runtime state is initialized.
- `EntityRenderManager.OnCreateEntity(...)` only handles actors (`PlayerDataComp`) and ignores bullet entities.
- `BulletManager.Tick(...)` iterates bullets but does not move, collide, trigger callbacks, or remove bullets.
- Entity removal does not currently guarantee render object destruction for bullets.

Result: bullet data exists, but bullets cannot complete a full lifecycle (spawn -> fly -> hit/expire -> remove).

## 2. Goals and Non-Goals

### Goals

1. Complete bullet lifecycle in fixed-step logic (`Tick`) at 15 FPS.
2. Keep default hit behavior as **"only trigger `OnHit` hook"** (no automatic hurt behavior switching).
3. Make bullet creation/removal synchronized between logic and render entities.
4. Preserve existing Ability framework conventions (action-driven creation, manager-driven ticking).

### Non-Goals

- No damage system redesign.
- No buff/ability behavior redesign outside bullet integration points.
- No vendor/third-party code modification.

## 3. Approach Comparison

### A) Logic-driven collision query (recommended)

Use `BulletManager.Tick` to move bullets and perform physics queries (`SphereCast`/`OverlapSphere`) for hit detection.

- Pros: deterministic with fixed-step; easier anti-tunneling; clean control over hit delay/hit counts.
- Cons: needs explicit collider-to-entity resolution path.

### B) Trigger callback-driven hit (`OnTriggerEnter`)

Reuse `HitBox` trigger callbacks as bullet hit source.

- Pros: less code in `BulletManager`.
- Cons: timing/callback ordering coupled to Unity physics update; harder to control multi-hit and same-target cooldown.

### C) Hybrid

Use trigger for entity hit and query for obstacle hit.

- Pros: moderate migration cost.
- Cons: split responsibility increases complexity and debugging cost.

### Decision

Adopt **A (logic-driven collision query)** for consistent frame-based gameplay behavior.

## 4. Architecture Changes

### 4.1 Data Model

#### `BulletData` (`Assets/Scripts/Ability/Actor/BulletData.cs`)

Align script fields with live asset data and bullet use cases:

- Keep/ensure fields: `Id`, `Prefab`, `Radius`, `HitTimes`, `HitSameDelay`, `RemoveOnObstacle`, `HitFoe`, `HitAlly`, `OnCreate`, `OnHit`, `OnRemoved`.
- Add/confirm runtime-driving fields used by assets:
  - `Speed`
  - `Duration`
  - `SpawnOffset`

#### `BulletDataComp` (`Assets/Scripts/Ability/Actor/BulletDataComp.cs`)

Add runtime state for deterministic bullet simulation:

- `Vector3 Position`
- `Vector3 Direction` (normalized)
- `float Speed`
- `float Duration`
- `float TimeElapsed`
- `int Hp` (initialized from `HitTimes`)
- `Entity Caster`
- same-target cooldown cache (e.g., `Dictionary<int, float> lastHitTimeByUid`)

#### `BulletAction` (`Assets/Scripts/Ability/Actor/BulletAction.cs`)

Upgrade execution contract so hooks can receive runtime context:

- Current: parameterless `Execute()`
- Target: `Execute(BulletDataComp bullet, Entity target)`
  - `target` is `null` for create/remove events
  - `target` is hit entity for `OnHit`

This keeps default behavior simple while giving extension points enough context.

### 4.2 Spawn Flow

#### `BulletLaunchAction` (`Assets/Scripts/Ability/Action/BulletLaunchAction.cs`)

On action enter (single frame):

1. Resolve caster entity via `tree.Entity`.
2. Read caster transform from `TransfromComp`.
3. Build launcher payload with:
   - bullet id
   - spawn position = caster position + rotated `SpawnOffset`
   - fire direction (caster forward or configured direction)
   - caster reference
4. Call `FightManager.LogicEntity.CreateBullet(...)`.

### 4.3 Entity Creation / Render Binding

#### `EntityManager.CreateBullet(...)`

Initialize all bullet runtime fields in `BulletDataComp` and dispatch create event (same as actor path):

- set config and dynamic state
- fire `EventId.CreateEntity`

#### `EntityRenderManager.OnCreateEntity(...)`

Extend create handler to support bullets:

- if entity has `PlayerDataComp`: keep actor flow unchanged
- if entity has `BulletDataComp`: instantiate `BulletData.Prefab`
- attach render sync component for bullets (new `BulletRenderTransformComp` or equivalent)

### 4.4 Tick / Movement / Collision

#### `BulletManager.Tick(...)`

For each alive bullet:

1. Increase `TimeElapsed`; if exceeded `Duration`, run remove flow.
2. Compute `nextPos = Position + Direction * Speed * deltaTime`.
3. Sweep along movement path with bullet radius:
   - detect `HurtBox` targets
   - detect obstacle colliders
4. For each valid target hit:
   - skip self / invalid entity / team-filter mismatch (`HitFoe`, `HitAlly`)
   - enforce same-target cooldown (`HitSameDelay`)
   - call `Data.OnHit?.Execute(bulletComp, target)`
   - decrement `Hp`
   - if `Hp <= 0`, remove bullet immediately
5. If obstacle hit and `RemoveOnObstacle`, remove bullet.
6. If not removed, commit `Position = nextPos`.

### 4.5 Removal Synchronization

Add a remove event for render cleanup:

- extend `EventId` with `RemoveEntity`
- when logic entity removed, notify `RemoveEntity(uid)`
- `EntityRenderManager` listens and removes render entity
- `EntityRender.Destroy()` destroys bound GameObject

This avoids leaked bullet GameObjects.

## 5. Data Flow Summary

1. Ability enters frame window -> `BulletLaunchAction` executes.
2. Action requests logical bullet creation with launch context.
3. Logic bullet created (`EntityType.Bullet`) + runtime component initialized.
4. Create event spawns render GameObject and binds render sync component.
5. `BulletManager.Tick` drives movement/collision/lifecycle.
6. On hit: only `OnHit` hook is triggered by default.
7. On expiration/hit limit/obstacle: `OnRemoved` hook -> logic remove -> render remove.

## 6. Error Handling and Safety

- Null-safe checks for config, components, and mapped target entities.
- Ignore invalid colliders without `IdentitCard`/uid mapping.
- Defensive handling when prefab is missing: log error and still keep logic entity removable.
- Process bullet linked list safely when removing current node (store `next` before remove).
- Clamp/validate non-positive config values (`Duration <= 0`, `Speed < 0`, `HitTimes <= 0`).

## 7. Test Strategy

### Manual gameplay checks

1. Single bullet spawns at expected offset and travels forward.
2. Bullet expires by duration and triggers `OnRemoved`.
3. Bullet hits enemy and triggers `OnHit` (without forced hurt behavior by default).
4. `HitTimes > 1` allows multiple target hits until depleted.
5. `HitSameDelay` prevents repeated fast hits on same target.
6. Obstacle collision removes bullet only when `RemoveOnObstacle = true`.
7. Logic removal always removes render GameObject.

### Regression checks

- Existing melee `AttackComp/HitBox` flow remains unchanged.
- Actor rendering and movement unaffected.

## 8. Implementation Steps (high-level)

1. Align bullet data structures (`BulletData`, `BulletDataComp`, `BulletAction`).
2. Implement spawn path (`BulletLaunchAction` + `EntityManager.CreateBullet`).
3. Extend render creation/removal path for bullet entities.
4. Implement `BulletManager` lifecycle, movement, and collision logic.
5. Add/remove events and cleanup wiring.
6. Run manual verification scenarios in `AbilityTest.unity`.
