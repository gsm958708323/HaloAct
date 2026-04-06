# Buff / Bullet Demo Task Plan

## Goal

在 HaloAct 现有 EC 架构下，继续完善文章式 Buff 系统与子弹系统的接入，并产出一个可运行的最小 Demo。

## Architectural Principles

- 第一原则：`流程骨架稳定，玩法逻辑外挂`
- `Entity = Obj`
- `BuffData / BulletData / ActorData = Model`
- `DamageInfo / AddBuffInfo = Info`
- 新玩法优先落在：
  - `DamageInfo`
  - `AddBuffInfo`
  - `EffectComp` 分发
  - `BuffAction / BulletAction`
- 避免把 Demo 逻辑硬编码进：
  - `AttackComp`
  - `BulletManager`
  - `Main`
- 上述核心类只承担调度、接线和上下文组装职责

## Assumptions

- Demo 继续使用 `Assets/Scenes/AbilityTest.unity`
- 不引入全局 `BuffManager`
- 尽量复用现有资源：
  - `Assets/Scripts/Ability/Resources/Actor/1001.asset`
  - `Assets/Scripts/Ability/Resources/Actor/2001.asset`
  - `Assets/Scripts/Ability/Resources/Actor/1001_ComboGraph.asset`
  - `Assets/Scripts/Ability/Resources/Actor/2001_ComboGraph.asset`
  - `Assets/Scripts/Ability/Resources/Bullet/3001.asset`
  - `Assets/Scripts/Ability/Resources/Buff/1001.asset`
  - `Assets/Scripts/Ability/Resources/Ability/1001/Behavior/Skill01.asset`

## Demo Target

`Skill01 -> Bullet/3001 -> Buff/1001 -> 改写 DamageInfo.AttackType -> Enemy HurtFly/HurtDown`

说明：
1. 玩家按键 `1` 触发 `Skill01`
2. `Skill01` 发射 `3001` 子弹
3. 子弹命中敌人后给敌人添加 `1001` Buff
4. Buff 通过流程回调改写后续受击 `DamageInfo`
5. 敌人后续受击从 `HurtNormal` 切到 `HurtFly` 或 `HurtDown`

## Phases

### Phase 1: Context Sync
Status: completed

- 已确认现有 Buff / Bullet 改动属于“部分接入”状态
- 已确认 `Skill01` 节点和 `HurtNormal / HurtDown / HurtFly` 节点存在
- 已确认 `AddBuffAction` 仍为空实现

### Phase 2: Task Shaping
Status: completed

- 已收敛 Demo 目标链路
- 已确认实现必须遵守文章的扩展性思路，而不是只追求功能跑通

### Phase 3: Subagent Implementation
Status: completed

子任务 A：补齐运行时扩展点
- 完成 `AddBuffAction`
- 必要时新增“子弹命中后添加 Buff”的 `BulletAction` 子类
- 修正 Buff / Bullet / DamageInfo 交互中的缺口

子任务 B：接通 Demo 配置链
- 让 `Skill01` 真正发射子弹
- 让 `Bullet/3001` 命中后给敌人加 `Buff/1001`
- 让 `Buff/1001` 通过回调改写后续受击类型

子任务 C：验证与说明
- 补齐对应 EditMode 测试
- 给出 Demo 触发方式和验证步骤

### Phase 4: Review And Verification
Status: completed

- 审核子代理结果是否遵守“流程骨架稳定，玩法逻辑外挂”
- 运行项目建议的验证命令
- 总结剩余风险

已完成的验证：
- `tools/Run-HaloActEditModeTests.ps1 -TestClass Ability.Buff.BuffBulletDemoAssetSyncTests`
- `tools/Run-HaloActEditModeTests.ps1 -TestNamespace Ability.Buff`

## Validation Strategy Update (2026-04-05)

- Unity EditMode 验证以 `tools/Run-HaloActEditModeTests.ps1` 为唯一主入口
- 不在 batchmode 中走 `-executeMethod HaloFrame.Editor.HaloActEditModeBatchRunner.RunFromCommandLine`
- 测试产物固定写入 `TestArtifacts/TestRunner`，不写入 `Temp/`
- Unity batchmode 测试必须串行执行，并确保 Unity Editor 未打开当前工程
- 选测时优先使用 `-TestClass`、`-TestClass + -TestMethod`、`-TestNamespace`，仅在必要时退回 `-TestFilter`
- `dotnet build` 只保留为辅助编译检查，不再作为“测试已通过”的依据

## Risks

- 当前工作区有未提交改动，不能误覆盖用户工作
- `dotnet build` 与 Unity batchmode 验证链路需要分层看待
- `Entity.DeathCheck()` 仍为空，任何击杀闭环都不能被假定为已完成
- 还没有做人工场景烟测；`Assets/Scenes/AbilityTest.unity` 的“按 1 演示”链路目前由资源接线和 EditMode 测试间接保证

基于这篇文章我之前提炼出的主线能力，以及当前主干代码实现，结论可以直接分成三部分。

**已实现**
- `Buff` 基本运行时闭环已经有了
  - 支持 `AddBuffInfo -> EffectComp -> EffectObj`
  - 支持排队添加、统一 `FlushPending()`、基础叠加/刷新/移除
  - 支持 `OnCast / OnTick / OnRemoved / OnOccur`
  - 已接入 `OnHit / OnBeHurt / OnKill / OnBeKilled`
  - 代码：[EffectComp.cs]( Actor/EffectComp.cs) [EffectObj.cs]( Actor/EffectObj.cs) [BuffData.cs]( Actor/BuffData.cs)
- 运行时属性层已经补上
  - 新增 `AttrComp`
  - Buff 能通过 `BuffModifierGroup` 重算属性和控制状态
  - 代码：[AttrComp.cs]( Actor/AttrComp.cs) [ActorAttr.cs]( GameDefine/ActorAttr.cs) [BuffModifierGroup.cs]( GameDefine/BuffModifierGroup.cs)
- 统一伤害流水线已经建立
  - 新增 `DamageInfo + DamageManager`
  - 近战和子弹都能走统一伤害入口
  - 支持伤害回调里再排队额外伤害
  - 代码：[DamageManager.cs]( Damage/DamageManager.cs) [DamageInfo.cs]( Damage/DamageInfo.cs) [AttackComp.cs]( Actor/AttackComp.cs)
- 子弹生命周期大部分实现了
  - 支持 `Launcher -> Entity -> BulletDataComp -> BulletManager`
  - 支持命中、撞墙、寿命结束、命中次数耗尽
  - 支持 `CanHitAfterCreated`
  - 支持 removal reason
  - 代码：[BulletManager.cs]( Manager/BulletManager.cs) [BulletDataComp.cs]( Actor/BulletDataComp.cs) [BulletData.cs]( Actor/BulletData.cs)
- `AOE` 子系统已经落地
  - 支持 `AoeData / AoeDataComp / AoeManager / AoeLaunchAction`
  - 支持 `OnCreate / OnEnter / OnTick / OnLeave / OnRemoved`
  - 支持作用于 Actor 和 Bullet
  - 代码：[AoeManager.cs]( Manager/AoeManager.cs) [AoeDataComp.cs]( Actor/AoeDataComp.cs) [AoeData.cs]( Actor/AoeData.cs)
- 数据驱动基础已经补上
  - `BuffData / BulletData / AoeData` 都能走配置
  - 新增 `CombatDataValidator`
  - 代码：[ConfigManager.cs]( Manager/ConfigManager.cs) [CombatDataValidator.cs]( Editor/Buff/CombatDataValidator.cs)
- 性能上做了第一层优化
  - `BulletManager` 用了 `SphereCastNonAlloc`
  - `AoeManager` 用了 `OverlapSphereNonAlloc`
  - 代码：[BulletManager.cs]( Manager/BulletManager.cs) [AoeManager.cs]( Manager/AoeManager.cs)

**没实现完整**
- 文章里更完整的“运行时可改写 model/shadow copy”只做了一半
  - 现在 Buff 支持运行时请求数据
  - 但 Bullet/AOE 还是主要依赖 `ScriptableObject + 少量 shadow state`
  - 还没到文章那种运行中可安全派生完整 model 的程度
- 真正的数值结算还没完成
  - `DamageManager` 有事件流，但没有完整 `HP/Defense/最终伤害公式`
  - `IsLethal` 还是上下文标记，不是由真实血量推导
  - 代码：[DamageInfo.cs]( Damage/DamageInfo.cs)
- Buff 语义还不够完整
  - 有叠加/刷新/移除
  - 但“同 id + 同 caster 才叠加”这类更细规则还没单独建模
  - buff 标签筛选、免疫、驱散体系也没做完整
- AOE 还没真正接入统一伤害语义
  - 现在 AOE 主要是通用回调层
  - 没形成像 Bullet 一样自然产出 `DamageInfo` 的标准模式
- 数据驱动还停留在“结构安全”
  - 校验了 ID、Tick 回调、基础约束
  - 还没做资源扫描、引用完整性、批量编辑器入口、配置依赖检查
- 表现层和资源资产还没补齐
  - runtime 有了
  - 但真正可用的 AOE/Buff/Bullet 配套资产和编辑器工作流还不完整

**设计上还存在的缺陷**
- `DamageManager` 统一了入口，但语义仍偏“事件分发器”，不是完整战斗裁决器
  - 没有数值快照
  - 没有 damage result
  - 没有标准化的 hit result / remove reason / death reason 结果对象
- `EffectComp` 仍然是“按 buffId 做唯一键”
  - 这和文章里更细的 buff instance 心智还有差距
  - 后面遇到“同 buffId 不同施法者并存”会受限
  - 代码：[EffectComp.cs]( Actor/EffectComp.cs)
- `BulletManager` 和 `AoeManager` 还比较依赖 Unity 物理世界
  - 这兼容现有项目，但逻辑确定性和大规模扩展性一般
  - 如果以后想做纯逻辑模拟、回放或服务器战斗，会成为约束
- `NonAlloc` 缓冲区是固定长度
  - 当前是实用优化
  - 但命中体很多时会截断结果
  - 代码：[BulletManager.cs]( Manager/BulletManager.cs) [AoeManager.cs]( Manager/AoeManager.cs)
- `master` 分支现在虽然有提交记录，但你主工作区文件状态之前经历过 `update-ref`
  - 分支历史是对的
  - 工作区同步要再谨慎检查一次，别把“分支正确”和“工作树完全同步”混为一谈

**一句话总结**
- 文章里你最关心的四块：`子弹生成与销毁`、`Buff 叠加/刷新/移除`、`数据驱动配置`、`性能和可扩展性`
- 现在都已经“实现到可跑、可测、可继续扩展”的层级了
- 但还没有达到文章那种“完整战斗框架成品”的程度，尤其缺少完整数值结算、标准化结果对象、以及更强的运行时 model 改写能力

如果你要，我下一步可以继续给你一份更尖锐的版本：
1. “哪些地方如果现在上线，最容易出 bug”
2. “下一轮最该补的 5 个点，按优先级排好”

