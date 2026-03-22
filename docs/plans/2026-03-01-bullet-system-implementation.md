# Bullet System Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a complete frame-driven bullet lifecycle (spawn, move, hit, expire, remove) where default hit behavior only triggers `OnHit` hook logic.

**Architecture:** Keep bullet simulation in fixed-step `Tick` (`BulletManager`) and keep rendering as a mirror of logic entities. Bullet creation starts from `BulletLaunchAction`, goes through `EntityManager`, and dispatches events for render spawn/cleanup. Collision is logic-driven (`SphereCast`) so hit count and same-target delay remain deterministic at 15 FPS.

**Tech Stack:** Unity 2021.3, C#, HaloFrame manager loop (`IManager`), Unity Physics queries, Unity Test Framework (EditMode + PlayMode).

---

### Task 1: Lock Bullet Data Contract + Test Scaffolding

**Files:**
- Create: `Assets/Tests/EditMode/HaloAct.EditModeTests.asmdef`
- Create: `Assets/Tests/EditMode/Bullet/BulletDataContractTests.cs`
- Modify: `Assets/Scripts/Ability/Actor/BulletData.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void BulletData_MustContainRuntimeFields()
{
    var t = typeof(BulletData);
    Assert.NotNull(t.GetField("Speed"));
    Assert.NotNull(t.GetField("Duration"));
    Assert.NotNull(t.GetField("SpawnOffset"));
}
```

**Step 2: Run test to verify it fails**

Run:
`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform EditMode -testFilter "Ability.Tests.BulletDataContractTests" -testResults "Logs/EditMode-BulletDataContract.xml"`

Expected: FAIL with missing field assertions.

**Step 3: Write minimal implementation**

```csharp
public float Speed;
public float Duration;
public Vector3 SpawnOffset;
```

**Step 4: Run test to verify it passes**

Run same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add "Assets/Tests/EditMode/HaloAct.EditModeTests.asmdef" "Assets/Tests/EditMode/Bullet/BulletDataContractTests.cs" "Assets/Scripts/Ability/Actor/BulletData.cs"
git commit -m "test: lock bullet data contract fields"
```

### Task 2: Make Bullet Hooks Context-Aware + Spawn from Ability Action

**Files:**
- Create: `Assets/Tests/EditMode/Bullet/BulletActionAndLaunchTests.cs`
- Modify: `Assets/Scripts/Ability/Actor/BulletAction.cs`
- Modify: `Assets/Scripts/Ability/Actor/BulletDataComp.cs`
- Modify: `Assets/Scripts/Ability/Action/BulletLaunchAction.cs`
- Modify: `Assets/Scripts/Ability/Manager/EntityManager.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void BulletAction_Execute_PassesBulletAndTarget()
{
    var action = new ProbeBulletAction();
    var comp = new BulletDataComp();
    var target = new Entity();

    action.Execute(comp, target);

    Assert.AreSame(comp, action.LastBullet);
    Assert.AreSame(target, action.LastTarget);
}
```

Also add a test that `BulletLaunchAction` calls `CreateBullet` exactly once on enter frame.

**Step 2: Run test to verify it fails**

Run:
`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform EditMode -testFilter "Ability.Tests.BulletActionAndLaunchTests" -testResults "Logs/EditMode-BulletActionAndLaunch.xml"`

Expected: FAIL because `BulletAction.Execute` has no parameters and launch action does not spawn.

**Step 3: Write minimal implementation**

```csharp
public void Execute(BulletDataComp bullet, Entity target)
{
    this.bullet = bullet;
    this.target = target;
    OnExecute();
}
```

Implement launch path:
- build launch context from `tree.Entity` + `TransfromComp` + `BulletData.SpawnOffset`
- call `FightManager.LogicEntity.CreateBullet(...)`
- initialize runtime fields in `BulletDataComp`

**Step 4: Run test to verify it passes**

Run same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add "Assets/Tests/EditMode/Bullet/BulletActionAndLaunchTests.cs" "Assets/Scripts/Ability/Actor/BulletAction.cs" "Assets/Scripts/Ability/Actor/BulletDataComp.cs" "Assets/Scripts/Ability/Action/BulletLaunchAction.cs" "Assets/Scripts/Ability/Manager/EntityManager.cs"
git commit -m "feat: wire bullet launch and hook context"
```

### Task 3: Implement Deterministic Bullet Lifetime + Movement

**Files:**
- Create: `Assets/Tests/EditMode/Bullet/BulletMovementLifecycleTests.cs`
- Modify: `Assets/Scripts/Ability/Manager/BulletManager.cs`
- Modify: `Assets/Scripts/Ability/Actor/BulletDataComp.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void BulletManager_MovesBulletAndExpiresByDuration()
{
    // Arrange bullet with position=(0,0,0), dir=forward, speed=10, duration=0.1
    // Tick twice with dt=0.0666f
    // Assert moved and removed after duration exceeded
}
```

**Step 2: Run test to verify it fails**

Run:
`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform EditMode -testFilter "Ability.Tests.BulletMovementLifecycleTests" -testResults "Logs/EditMode-BulletMovementLifecycle.xml"`

Expected: FAIL because manager currently does not update lifetime/position.

**Step 3: Write minimal implementation**

```csharp
comp.TimeElapsed += deltaTime;
if (comp.TimeElapsed >= comp.Duration) RemoveBullet(bullet);
comp.Position += comp.Direction * comp.Speed * deltaTime;
```

Ensure safe linked-list iteration while removing current bullet.

**Step 4: Run test to verify it passes**

Run same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add "Assets/Tests/EditMode/Bullet/BulletMovementLifecycleTests.cs" "Assets/Scripts/Ability/Manager/BulletManager.cs" "Assets/Scripts/Ability/Actor/BulletDataComp.cs"
git commit -m "feat: add bullet movement and duration lifecycle"
```

### Task 4: Add Hit Filtering, Same-Target Delay, and `OnHit`-Only Default Behavior

**Files:**
- Create: `Assets/Tests/PlayMode/HaloAct.PlayModeTests.asmdef`
- Create: `Assets/Tests/PlayMode/Bullet/BulletCollisionRulesTests.cs`
- Modify: `Assets/Scripts/Ability/Manager/BulletManager.cs`
- Modify: `Assets/Scripts/Ability/Actor/BulletDataComp.cs`

**Step 1: Write the failing test**

```csharp
[UnityTest]
public IEnumerator Bullet_HitsEnemy_TriggersOnHit_DecrementsHp_RespectsHitSameDelay()
{
    // Setup caster + target + hurtbox colliders
    // Fire bullet across target twice in short interval
    // Assert OnHit count == 1 during cooldown
    // Wait beyond delay, assert second hit allowed
}
```

Add assertions that no automatic hurt behavior switch is forced by bullet manager.

**Step 2: Run test to verify it fails**

Run:
`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform PlayMode -testFilter "Ability.Tests.BulletCollisionRulesTests" -testResults "Logs/PlayMode-BulletCollisionRules.xml"`

Expected: FAIL because no collision filtering and cooldown logic exists.

**Step 3: Write minimal implementation**

```csharp
if (!CanHitTarget(caster, target, data.HitFoe, data.HitAlly)) return;
if (!PassHitSameDelay(target.Uid, now, data.HitSameDelay)) return;
data.OnHit?.Execute(comp, target);
comp.Hp -= 1;
```

Collision source:
- use `Physics.SphereCastAll` from current to next position with bullet radius
- resolve `IdentitCard` -> `LogicEntity`
- apply obstacle removal when `RemoveOnObstacle` is true

**Step 4: Run test to verify it passes**

Run same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add "Assets/Tests/PlayMode/HaloAct.PlayModeTests.asmdef" "Assets/Tests/PlayMode/Bullet/BulletCollisionRulesTests.cs" "Assets/Scripts/Ability/Manager/BulletManager.cs" "Assets/Scripts/Ability/Actor/BulletDataComp.cs"
git commit -m "feat: implement bullet collision rules and onhit behavior"
```

### Task 5: Sync Bullet Render Spawn/Despawn with Logic Entity Lifecycle

**Files:**
- Create: `Assets/Tests/PlayMode/Bullet/BulletRenderSyncTests.cs`
- Create: `Assets/Scripts/Ability/Actor/BulletRenderTransformComp.cs`
- Modify: `Assets/Scripts/Ability/Manager/EntityRenderManager.cs`
- Modify: `Assets/Scripts/Ability/GameDefine/EventId.cs`
- Modify: `Assets/Scripts/Ability/Manager/IEntityManager.cs`
- Modify: `Assets/Scripts/Ability/Actor/EntityRender.cs`

**Step 1: Write the failing test**

```csharp
[UnityTest]
public IEnumerator Bullet_RemoveLogicEntity_RemovesRenderGameObject()
{
    // Create bullet entity
    // Remove it from logic manager
    // Assert render entity and instantiated prefab are destroyed
}
```

**Step 2: Run test to verify it fails**

Run:
`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform PlayMode -testFilter "Ability.Tests.BulletRenderSyncTests" -testResults "Logs/PlayMode-BulletRenderSync.xml"`

Expected: FAIL because render cleanup path is incomplete.

**Step 3: Write minimal implementation**

```csharp
public const int RemoveEntity = 2;
GameManager.Dispatcher.Notify<int>(EventId.RemoveEntity, uid);
```

Implement:
- render manager listens for remove event and removes render entity
- `EntityRender.Destroy()` destroys bound GameObject
- bullet render transform mirrors `BulletDataComp.Position/Direction`

**Step 4: Run test to verify it passes**

Run same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add "Assets/Tests/PlayMode/Bullet/BulletRenderSyncTests.cs" "Assets/Scripts/Ability/Actor/BulletRenderTransformComp.cs" "Assets/Scripts/Ability/Manager/EntityRenderManager.cs" "Assets/Scripts/Ability/GameDefine/EventId.cs" "Assets/Scripts/Ability/Manager/IEntityManager.cs" "Assets/Scripts/Ability/Actor/EntityRender.cs"
git commit -m "feat: sync bullet render lifecycle with logic entities"
```

### Task 6: Final Verification and Documentation Update

**Files:**
- Modify: `docs/plans/2026-03-01-bullet-system-design.md` (mark implemented details and any deltas)

**Step 1: Write the failing check list**

Create a verification checklist in the design doc with unchecked items for:
- spawn
- move
- hit
- same-target delay
- duration remove
- obstacle remove
- render cleanup

**Step 2: Run test suites before implementation complete**

Run:
- `"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform EditMode -testResults "Logs/EditMode-All.xml"`
- `"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform PlayMode -testResults "Logs/PlayMode-All.xml"`

Expected: FAIL before all code/tasks are completed.

**Step 3: Complete remaining implementation gaps**

Only minimal fixes required to pass all checks; avoid adding extra feature scope.

**Step 4: Re-run full verification**

Run same two commands as Step 2.

Expected: PASS on EditMode + PlayMode suites.

**Step 5: Commit**

```bash
git add "docs/plans/2026-03-01-bullet-system-design.md"
git commit -m "docs: update bullet design with implementation verification"
```
