# Findings

## 2026-04-05

### Buff / Bullet 当前状态

- `EffectComp` 已有 `DispatchOnHit`、`DispatchOnBeHurt`、`DispatchOnKill`、`DispatchOnBeKilled` 入口，说明文章式回调骨架已部分接入
- `BulletManager` 当前已经通过 `DamageInfo` + `DamageResolver` 处理命中，并用 `TriggerHurtBehavior` / `ConsumeHit` 控制受击动作和子弹消耗
- `Main.Start()` 仍然只创建 `1001` 玩家和 `2001` 敌人，Demo 入口适合直接复用这条链路
- `FightManager` 已负责初始化 `ConfigManager`、`EntityManager`、`EntityRenderManager`、`BulletManager` 和 `PlayerGameInput`

### 当前缺口

- 现有计划文档偏向“核心骨架接入”，还没有覆盖“场景级 Demo 如何演示 Buff 和子弹联动”
- `git diff --stat` 显示 Buff / Bullet 关键文件已被修改，但还没有形成完整的 Demo 交付闭环
- 先前测试基础设施有过 `.csproj` / 引用链问题，本轮验证需要把“代码测试”和“Unity 场景演示”分开看待
- `AddBuffAction.OnExecute()` 当前仍为空实现，内容层还不能靠配置真正把 Buff 加到目标实体上
- `Assets/Scripts/Ability/Resources/Ability/1001/Behavior/Skill01.asset` 当前序列化里 `Attacks` 长度为 `0`，说明现有玩家技能资源并没有直接提供可演示的子弹/命中行为
- `Assets/Scripts/Ability/Resources/Actor`、`Buff`、`Bullet` 下已经有 `1001`、`2001`、`3001` 等基础资源，适合在现有资源体系内补一个最小 Demo，而不是新造平行资源系统

### 本轮默认设计决策

- Demo 不新开场景，直接落在 `Assets/Scenes/AbilityTest.unity` 的启动链路
- 继续坚持 EC 对照关系：
  - `BuffData = BuffModel`
  - `EffectObj = BuffObj`
  - `EffectComp = 实体级 Buff 容器`
  - `DamageInfo = 流程上下文`
- 用户新增明确约束：实现思路也要尽量遵守文章内容，不能只追求“功能跑通”
- 用户新增第一原则：`流程骨架稳定，玩法逻辑外挂`
- 因此实现应优先落在 `DamageInfo`、`AddBuffInfo`、`EffectComp` 分发、`BuffAction / BulletAction` 等扩展点上
- 若某个 Demo 方案需要在 `AttackComp`、`BulletManager`、`Main` 中硬编码大量特判，则应视为违背文章思路，优先换成更“流程驱动”的方案

### 测试链路约束

- 项目 `AGENTS.md` 已明确 Unity EditMode 测试默认入口是 `tools/Run-HaloActEditModeTests.ps1`
- 项目 `AGENTS.md` 明确禁止在 batchmode 里走 `-executeMethod HaloFrame.Editor.HaloActEditModeBatchRunner.RunFromCommandLine`
- 测试结果、日志和摘要应落在 `TestArtifacts/TestRunner`，而不是 `Temp/`
- 同一工程的 Unity batchmode 测试必须串行执行，且不能与打开中的 Unity Editor 争锁
- 这意味着本轮验证要区分：
  - `dotnet build` 仅用于辅助确认 C# 编译链
  - Unity EditMode 真正执行必须走项目脚本
