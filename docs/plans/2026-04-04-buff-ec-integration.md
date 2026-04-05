# HaloAct Buff（EC 对照）Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 在不破坏 HaloAct 现有 `Entity + Component` 架构的前提下，把文章式 Buff 设计接入现有 Actor/Bullet 战斗链路，补齐 `DamageInfo`、`OnHit`、`OnBeHurt` 等核心回调，并保持 Buff 由实体局部持有。

**Architecture:** 继续使用 EC：`BuffData` 作为静态模板，`EffectObj` 作为运行时实例，`EffectComp` 作为实体自己的 Buff 容器。新增 `DamageInfo` 和 `DamageResolver` 作为流程上下文与调度 helper，把 `AttackComp`、`HurtBox`、`BulletManager` 接到同一条 Buff 处理链上；`OnKill`/`OnBeKilled` 只先落接口和分发点，不强行接入当前尚未成型的 HP/死亡系统。

**Tech Stack:** Unity 2021.3, C#, ScriptableObject, Assembly-CSharp / Assembly-CSharp-Editor, Unity EditMode tests.

---

## EC 对照

- 文章里的 `CharacterObj / ChaState`：对照为 HaloAct 的 `Entity` 容器加一组组件
  - `PlayerDataComp`：静态角色配置
  - `TransfromComp`：位置、朝向、运动状态
  - `BehaviorComp`：行为节点切换与连招运行时
  - `EffectComp`：实体局部 Buff 容器
  - `AttackComp`：近战命中/受击入口
- 文章里的 `BuffModel`：对照为 `BuffData`
- 文章里的 `BuffObj`：对照为 `EffectObj`
- 文章里的 `AddBuffInfo`：继续保留为运行时命令对象
- 文章里的 `DamageInfo`：本计划新增，作为一次命中/受击流程的上下文，不挂在实体上
- 文章里的 `BuffManager`：不做全局拥有者；在 HaloAct 里由 `EffectComp` 承担每个实体自己的 Buff 管理职责

## 统一测试命令

本计划统一使用 `dotnet test` 跑 `EditMode` 测试工程，不再使用 Unity batchmode 命令。

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' `
  --filter 'FullyQualifiedName~Ability.Buff.EffectRuntimeTests' `
  --logger 'trx;LogFileName=buff-tests.trx'
```

如果第一次执行较慢，属于正常的 restore/build 成本；后续可以按需补 `--no-restore`。

### Task 1: 固化 EC 下的 Buff 运行时模型

**Files:**
- Modify: `Assets/Scripts/Ability/Actor/BuffData.cs`
- Modify: `Assets/Scripts/Ability/Actor/EffectObj.cs`
- Modify: `Assets/Scripts/Ability/Actor/EffectComp.cs`
- Test: `Assets/Tests/EditMode/Editor/Ability/Buff/EffectRuntimeTests.cs`

**Step 1: 写出失败测试**

```csharp
using NUnit.Framework;
using UnityEngine;

namespace Ability.Buff
{
    public class EffectRuntimeTests
    {
        [Test]
        public void AddBuff_SetsCreaterCarrierStackAndDuration()
        {
            var target = BuffTestHelpers.CreateActorEntity();
            var creater = BuffTestHelpers.CreateActorEntity();
            var effectComp = target.GetComp<EffectComp>();
            BuffTestHelpers.RegisterBuff(1001);

            var buff = effectComp.AddBuff(new AddBuffInfo
            {
                BuffId = 1001,
                Creater = creater,
                Target = target.Uid,
                AddStack = 1,
                Duration = 2f,
                IsOverrideDuration = true,
            });

            Assert.That(buff.Creater, Is.SameAs(creater));
            Assert.That(buff.Target, Is.SameAs(target));
            Assert.That(buff.Stack, Is.EqualTo(1));
            Assert.That(buff.Duration, Is.EqualTo(2f).Within(0.001f));
        }

        [Test]
        public void ExpiredBuff_OnRemovedIsInvokedExactlyOnce()
        {
            var target = BuffTestHelpers.CreateActorEntity();
            var effectComp = target.GetComp<EffectComp>();
            var counter = ScriptableObject.CreateInstance<TestRemovedCounterAction>();
            BuffTestHelpers.RegisterBuff(1002, onRemoved: counter);

            effectComp.AddBuff(new AddBuffInfo
            {
                BuffId = 1002,
                Creater = target,
                Target = target.Uid,
                AddStack = 1,
                Duration = 0.1f,
                IsOverrideDuration = true,
            });

            effectComp.Tick(0.2f);

            Assert.That(counter.Count, Is.EqualTo(1));
        }
    }
}
```

**Step 2: 运行测试，确认它失败**

Run:

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' --filter 'FullyQualifiedName~Ability.Buff.EffectRuntimeTests' --logger 'trx;LogFileName=buff-tests.trx'
```

Expected: FAIL，因为当前 `EffectObj` 没有完整暴露运行时字段，且 `OnRemoved` 会在 `EffectObj` 和 `EffectComp` 两处重复触发。

**Step 3: 写最小实现**

- 在 `EffectObj` 中补齐文章式运行时字段与只读访问：
  - `Creater`
  - `Target`
  - `Duration`
  - `TimeElapsed`
  - `Ticked`
  - `Param`
- 保持项目现有拼写 `Creater`，不要顺手改公共命名。
- `EffectComp.AddBuff(...)` 负责把 `AddBuffInfo` 写入 `EffectObj`，而不是只改栈和时长。
- 把 `OnRemoved` 的唯一触发点收敛到 `EffectComp`；`EffectObj.TickFinish(...)` 只返回“是否该移除”，不自己执行移除回调。
- 给 `EffectComp` 补最小的查询接口，方便后续测试与分发：
  - `bool HasBuff(int buffId)`
  - `EffectObj GetBuff(int buffId)`

**Step 4: 再跑测试，确认通过**

Run 同一个 `dotnet test` 命令。

Expected: PASS。

**Step 5: Commit**

```powershell
git add Assets/Scripts/Ability/Actor/BuffData.cs Assets/Scripts/Ability/Actor/EffectObj.cs Assets/Scripts/Ability/Actor/EffectComp.cs Assets/Tests/EditMode/Editor/Ability/Buff/EffectRuntimeTests.cs
git commit -m "refactor: align effect runtime with ec buff ownership"
```

### Task 2: 引入 DamageInfo 与文章式 Buff 回调骨架

**Files:**
- Create: `Assets/Scripts/Ability/Damage/DamageInfo.cs`
- Create: `Assets/Scripts/Ability/BuffAction/BuffHitAction.cs`
- Create: `Assets/Scripts/Ability/BuffAction/BuffBeHurtAction.cs`
- Create: `Assets/Scripts/Ability/BuffAction/BuffKillAction.cs`
- Create: `Assets/Scripts/Ability/BuffAction/BuffBeKilledAction.cs`
- Modify: `Assets/Scripts/Ability/Actor/BuffData.cs`
- Test: `Assets/Tests/EditMode/Editor/Ability/Buff/DamageInfoTests.cs`

**Step 1: 写出失败测试**

```csharp
using NUnit.Framework;

namespace Ability.Buff
{
    public class DamageInfoTests
    {
        [Test]
        public void DamageInfo_CanQueueBuffRequestsWithoutTouchingCarrierImmediately()
        {
            var attacker = BuffTestHelpers.CreateActorEntity();
            var defender = BuffTestHelpers.CreateActorEntity();
            var info = new DamageInfo(attacker, defender);

            info.QueueBuff(new AddBuffInfo
            {
                BuffId = 1001,
                Creater = attacker,
                Target = defender.Uid,
                AddStack = 1,
            });

            Assert.That(info.PendingBuffs, Has.Count.EqualTo(1));
            Assert.That(defender.GetComp<EffectComp>().HasBuff(1001), Is.False);
        }
    }
}
```

**Step 2: 运行测试，确认它失败**

Run:

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' --filter 'FullyQualifiedName~Ability.Buff.DamageInfoTests' --logger 'trx;LogFileName=buff-tests.trx'
```

Expected: FAIL，因为 `DamageInfo` 和新的 Buff 回调类型还不存在。

**Step 3: 写最小实现**

- 新建 `DamageInfo`，只放第一阶段真正需要的字段：
  - `Entity Attacker`
  - `Entity Defender`
  - `AbilityBehaviorAttack AttackBehavior`
  - `BulletDataComp Bullet`
  - `AttackType? AttackType`
  - `bool TriggerHurtBehavior = true`
  - `bool ConsumeHit = true`
  - `bool IsRejected`
  - `List<AddBuffInfo> PendingBuffs`
- 给 `DamageInfo` 提供 `QueueBuff(AddBuffInfo)` 方法。
- 扩展 `BuffData`，新增：
  - `BuffHitAction OnHit`
  - `BuffBeHurtAction OnBeHurt`
  - `BuffKillAction OnKill`
  - `BuffBeKilledAction OnBeKilled`
- `OnKill` / `OnBeKilled` 本阶段只建接口和基类，不接入真实死亡结算。

**Step 4: 再跑测试，确认通过**

Run 同一个 `dotnet test` 命令。

Expected: PASS。

**Step 5: Commit**

```powershell
git add Assets/Scripts/Ability/Damage/DamageInfo.cs Assets/Scripts/Ability/BuffAction/BuffHitAction.cs Assets/Scripts/Ability/BuffAction/BuffBeHurtAction.cs Assets/Scripts/Ability/BuffAction/BuffKillAction.cs Assets/Scripts/Ability/BuffAction/BuffBeKilledAction.cs Assets/Scripts/Ability/Actor/BuffData.cs Assets/Tests/EditMode/Editor/Ability/Buff/DamageInfoTests.cs
git commit -m "feat: add damage info and buff damage hooks"
```

### Task 3: 让 EffectComp 成为实体级 Buff 分发与排队中心

**Files:**
- Modify: `Assets/Scripts/Ability/Actor/EffectComp.cs`
- Modify: `Assets/Scripts/Ability/Actor/EffectObj.cs`
- Test: `Assets/Tests/EditMode/Editor/Ability/Buff/EffectDispatchTests.cs`

**Step 1: 写出失败测试**

```csharp
using NUnit.Framework;

namespace Ability.Buff
{
    public class EffectDispatchTests
    {
        [Test]
        public void DispatchOnBeHurt_FlushesQueuedBuffAfterIteration()
        {
            var attacker = BuffTestHelpers.CreateActorEntity();
            var defender = BuffTestHelpers.CreateActorEntity();
            BuffTestHelpers.RegisterQueueingBeHurtBuff(2001, 2002);
            BuffTestHelpers.RegisterBuff(2002);

            defender.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 2001,
                Creater = attacker,
                Target = defender.Uid,
                AddStack = 1,
            });

            var info = new DamageInfo(attacker, defender);
            defender.GetComp<EffectComp>().DispatchOnBeHurt(ref info);

            Assert.That(defender.GetComp<EffectComp>().HasBuff(2002), Is.True);
        }
    }
}
```

**Step 2: 运行测试，确认它失败**

Run:

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' --filter 'FullyQualifiedName~Ability.Buff.EffectDispatchTests' --logger 'trx;LogFileName=buff-tests.trx'
```

Expected: FAIL，因为 `EffectComp` 还没有 `DispatchOnBeHurt` 和统一 flush 机制。

**Step 3: 写最小实现**

- 给 `EffectComp` 增加统一分发入口：
  - `DispatchOnHit(ref DamageInfo info)`
  - `DispatchOnBeHurt(ref DamageInfo info)`
  - `DispatchOnKill(DamageInfo info)`
  - `DispatchOnBeKilled(DamageInfo info)`
- 分发时按 `BuffData.Priority` 顺序执行，保持和当前排序规则一致。
- 增加“延迟落地”机制：
  - 分发期间不直接改 `buffList`
  - 先把 `DamageInfo.PendingBuffs` 转成待执行请求
  - 分发结束后统一 `FlushPendingBuffs(...)`
- `EffectComp.AddBuff(...)` 保留为“立即应用”的底层 API；流程内新增请求全部走排队。
- 这里不要新建全局 `BuffManager`。

**Step 4: 再跑测试，确认通过**

Run 同一个 `dotnet test` 命令。

Expected: PASS。

**Step 5: Commit**

```powershell
git add Assets/Scripts/Ability/Actor/EffectComp.cs Assets/Scripts/Ability/Actor/EffectObj.cs Assets/Tests/EditMode/Editor/Ability/Buff/EffectDispatchTests.cs
git commit -m "feat: add effect dispatch and queued buff application"
```

### Task 4: 建立 DamageResolver，统一文章式命中流程

**Files:**
- Create: `Assets/Scripts/Ability/Damage/DamageResolver.cs`
- Modify: `Assets/Scripts/Ability/Actor/EffectComp.cs`
- Test: `Assets/Tests/EditMode/Editor/Ability/Buff/DamageResolverTests.cs`

**Step 1: 写出失败测试**

```csharp
using NUnit.Framework;

namespace Ability.Buff
{
    public class DamageResolverTests
    {
        [Test]
        public void Resolve_RunsAttackerOnHitBeforeDefenderOnBeHurt()
        {
            var attacker = BuffTestHelpers.CreateActorEntity();
            var defender = BuffTestHelpers.CreateActorEntity();
            var trace = new System.Collections.Generic.List<string>();

            BuffTestHelpers.RegisterTracingHitBuff(attacker, 3001, trace, "hit");
            BuffTestHelpers.RegisterTracingBeHurtBuff(defender, 3002, trace, "behurt");

            var info = new DamageInfo(attacker, defender);
            DamageResolver.Resolve(ref info);

            Assert.That(trace, Is.EqualTo(new[] { "hit", "behurt" }));
        }
    }
}
```

**Step 2: 运行测试，确认它失败**

Run:

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' --filter 'FullyQualifiedName~Ability.Buff.DamageResolverTests' --logger 'trx;LogFileName=buff-tests.trx'
```

Expected: FAIL，因为统一解析 helper 还不存在。

**Step 3: 写最小实现**

- 新建 `DamageResolver.Resolve(ref DamageInfo info)`：
  1. 如果攻击者有 `EffectComp`，先跑 `DispatchOnHit`
  2. 如果防守者有 `EffectComp`，再跑 `DispatchOnBeHurt`
  3. 统一 flush 双方排队请求
  4. 返回最终 `DamageInfo`
- 本阶段不在这里处理真实 HP 扣减，也不在这里切换受击节点。
- 只提供“流程顺序统一”和“回调入口统一”。
- 如果 `info.IsRejected == true`，后续调用方应自行决定是否还消费这次碰撞。

**Step 4: 再跑测试，确认通过**

Run 同一个 `dotnet test` 命令。

Expected: PASS。

**Step 5: Commit**

```powershell
git add Assets/Scripts/Ability/Damage/DamageResolver.cs Assets/Scripts/Ability/Actor/EffectComp.cs Assets/Tests/EditMode/Editor/Ability/Buff/DamageResolverTests.cs
git commit -m "feat: add shared damage resolver"
```

### Task 5: 接入近战链路（AttackComp / HurtBox）

**Files:**
- Modify: `Assets/Scripts/Ability/Actor/AttackComp.cs`
- Modify: `Assets/Scripts/Ability/CollisionBox/HurtBox.cs`
- Test: `Assets/Tests/EditMode/Editor/Ability/Buff/AttackCompDamageFlowTests.cs`

**Step 1: 写出失败测试**

```csharp
using NUnit.Framework;

namespace Ability.Buff
{
    public class AttackCompDamageFlowTests
    {
        [Test]
        public void AttackComp_UsesDamageResolverBeforeStartingHurtBehavior()
        {
            var attacker = BuffTestHelpers.CreateActorEntity();
            var defender = BuffTestHelpers.CreateActorEntityWithHurtBehavior(AttackType.Normal);
            BuffTestHelpers.RegisterRejectingBeHurtBuff(defender, 4001);

            defender.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 4001,
                Creater = defender,
                Target = defender.Uid,
                AddStack = 1,
            });

            var attack = BuffTestHelpers.CreateAttackBehavior(AttackType.Normal);
            BuffTestHelpers.RunDirectHurt(attacker, defender, attack);

            Assert.That(defender.GetComp<BehaviorComp>().curNode.Id, Is.Not.EqualTo(BuffTestHelpers.HurtNodeId));
        }
    }
}
```

**Step 2: 运行测试，确认它失败**

Run:

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' --filter 'FullyQualifiedName~Ability.Buff.AttackCompDamageFlowTests' --logger 'trx;LogFileName=buff-tests.trx'
```

Expected: FAIL，因为当前 `AttackComp` / `HurtBox` 直接切受击节点，没有先走 `DamageResolver`。

**Step 3: 写最小实现**

- 在 `AttackComp` 中组装 `DamageInfo`：
  - `Attacker = atkEntity`
  - `Defender = entity`
  - `AttackBehavior = atk`
  - `AttackType = atk.CurAttack.AttackType`
- 先保留原有格挡角度判定；格挡成功仍然走原有 `BlockEvents`。
- 非格挡分支改为：
  1. `DamageResolver.Resolve(ref info)`
  2. 若 `info.TriggerHurtBehavior == true`，再查 `GetHurtBehavior(...)` 并 `StartBehavior(...)`
- `HurtBox` 中重复的受击逻辑也同步改成同一流程，避免近战和碰撞两套逻辑继续分叉。
- 这里不要接入 HP 扣减；本阶段仅用 Buff 决定“是否进入受击行为”。

**Step 4: 再跑测试，确认通过**

Run 同一个 `dotnet test` 命令。

Expected: PASS。

**Step 5: Commit**

```powershell
git add Assets/Scripts/Ability/Actor/AttackComp.cs Assets/Scripts/Ability/CollisionBox/HurtBox.cs Assets/Tests/EditMode/Editor/Ability/Buff/AttackCompDamageFlowTests.cs
git commit -m "feat: route melee hurt flow through damage resolver"
```

### Task 6: 接入子弹链路（BulletManager）

**Files:**
- Modify: `Assets/Scripts/Ability/Manager/BulletManager.cs`
- Modify: `Assets/Scripts/Ability/Actor/BulletDataComp.cs`
- Test: `Assets/Tests/EditMode/Editor/Ability/Buff/BulletDamageFlowTests.cs`

**Step 1: 写出失败测试**

```csharp
using NUnit.Framework;

namespace Ability.Buff
{
    public class BulletDamageFlowTests
    {
        [Test]
        public void BulletHit_CanSuppressHurtBehaviorWithoutBreakingProjectileConsumption()
        {
            var shooter = BuffTestHelpers.CreateActorEntity();
            var target = BuffTestHelpers.CreateActorEntityWithHurtBehavior(AttackType.Normal);
            var bullet = BuffTestHelpers.CreateBulletEntity(shooter);
            BuffTestHelpers.RegisterRejectingBeHurtBuff(target, 5001);

            target.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 5001,
                Creater = target,
                Target = target.Uid,
                AddStack = 1,
            });

            var result = BuffTestHelpers.ResolveBulletHit(bullet, target);

            Assert.That(result.TriggerHurtBehavior, Is.False);
            Assert.That(result.ConsumeHit, Is.True);
        }
    }
}
```

**Step 2: 运行测试，确认它失败**

Run:

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' --filter 'FullyQualifiedName~Ability.Buff.BulletDamageFlowTests' --logger 'trx;LogFileName=buff-tests.trx'
```

Expected: FAIL，因为 `BulletManager` 还没有构建 `DamageInfo`。

**Step 3: 写最小实现**

- 在 `BulletManager.TryProcessCollision(...)` 命中目标时先构建 `DamageInfo`：
  - `Attacker = comp.Caster`
  - `Defender = target`
  - `Bullet = comp`
- 调用 `DamageResolver.Resolve(ref info)`。
- 当 `info.ConsumeHit == true` 时：
  - 记录命中
  - 执行子弹自身 `OnHit`
  - 扣减 `comp.Hp`
- 当 `info.TriggerHurtBehavior == true` 时：
  - 再触发受击行为切换
- 允许 Buff 只屏蔽受击动作而不屏蔽子弹碰撞消耗，这样和当前项目“子弹命中即消耗”的手感兼容。

**Step 4: 再跑测试，确认通过**

Run 同一个 `dotnet test` 命令。

Expected: PASS。

**Step 5: Commit**

```powershell
git add Assets/Scripts/Ability/Manager/BulletManager.cs Assets/Scripts/Ability/Actor/BulletDataComp.cs Assets/Tests/EditMode/Editor/Ability/Buff/BulletDamageFlowTests.cs
git commit -m "feat: route bullet hit flow through damage resolver"
```

### Task 7: 启用 AddBuffAction 并补一组最小冒烟用例

**Files:**
- Modify: `Assets/Scripts/Ability/BuffAction/AddBuffAction.cs`
- Create: `Assets/Tests/EditMode/Editor/Ability/Buff/AddBuffActionTests.cs`
- Create: `docs/plans/2026-04-04-buff-ec-smoke-checklist.md`

**Step 1: 写出失败测试**

```csharp
using NUnit.Framework;

namespace Ability.Buff
{
    public class AddBuffActionTests
    {
        [Test]
        public void AddBuffAction_AddsConfiguredBuffToTargetEffectComp()
        {
            var source = BuffTestHelpers.CreateActorEntity();
            var target = BuffTestHelpers.CreateActorEntity();
            BuffTestHelpers.RegisterBuff(6001);
            var effect = BuffTestHelpers.AttachRuntimeBuff(source, 6002);
            var action = BuffTestHelpers.CreateAddBuffAction(6001, target.Uid);

            action.Execute(effect);

            Assert.That(target.GetComp<EffectComp>().HasBuff(6001), Is.True);
        }
    }
}
```

**Step 2: 运行测试，确认它失败**

Run:

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' --filter 'FullyQualifiedName~Ability.Buff.AddBuffActionTests' --logger 'trx;LogFileName=buff-tests.trx'
```

Expected: FAIL，因为当前 `AddBuffAction.OnExecute()` 还是空实现。

**Step 3: 写最小实现**

- 在 `AddBuffAction.OnExecute()` 中：
  - 根据 `buffInfo.Target` 找到目标 `Entity`
  - 找到目标的 `EffectComp`
  - 调用 `AddBuff(...)`
- 保持第一版语义简单：
  - `Target` 继续用 `Uid`
  - 不引入“Self / Caster / LockedTarget”枚举
- 写一份手工冒烟清单 `2026-04-04-buff-ec-smoke-checklist.md`，记录：
  - 自身加 Buff
  - 目标加 Buff
  - `OnTick`
  - `OnRemoved`
  - 受击屏蔽动作
  - 子弹命中屏蔽动作

**Step 4: 跑一轮聚焦测试**

Run:

```powershell
dotnet test 'D:\Work\UnityProject\HaloAct\HaloAct.EditModeTests.csproj' --filter 'FullyQualifiedName~Ability.Buff' --logger 'trx;LogFileName=buff-tests.trx'
```

Expected: PASS。

**Step 5: Commit**

```powershell
git add Assets/Scripts/Ability/BuffAction/AddBuffAction.cs Assets/Tests/EditMode/Editor/Ability/Buff/AddBuffActionTests.cs docs/plans/2026-04-04-buff-ec-smoke-checklist.md
git commit -m "feat: enable add buff action and smoke checks"
```

## 非目标

- 不在这次计划里重建文章中的完整 Timeline/AoE 系统。
- 不在这次计划里补全真实 HP、数值伤害、击杀奖励逻辑。
- 不创建全局 `BuffManager` 持有所有 Buff。
- 不顺手修正项目里已有的公开拼写，例如 `Creater`。

## 完成前必须验证

- `EffectComp` 仍然是 Buff 的唯一实体级拥有者。
- `OnRemoved` 不会重复触发。
- 近战与子弹都走 `DamageResolver`，而不是各自直接切受击节点。
- `OnHit` / `OnBeHurt` 能通过 `DamageInfo` 排队修改 Buff。
- `OnKill` / `OnBeKilled` 至少已经有类型和分发入口，但未强行绑到空的 `DeathCheck()` 上。
- 所有新增测试都在 EditMode 下可重复执行。
