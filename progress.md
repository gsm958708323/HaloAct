# Progress

## 2026-04-05

### 会话启动

- 用户要求使用 `Planning with Files Zh` 继续完善 Buff 系统和子弹系统，并生成 Demo；代码编写使用 `unity3d-ec-architect` 子代理
- 已读取相关技能：`using-superpowers`、`planning-with-files-zh`、`brainstorming`、`subagent-driven-development`
- 已读取现有上下文：`docs/plans/2026-04-04-buff-ec-integration.md`、`EffectComp.cs`、`BulletManager.cs`、`Main.cs`、`FightManager.cs`

### 已确认信息

- 现有 Buff / Bullet 改动已经进入“部分接入”状态，不是从零开始
- Demo 最适合挂在现有 `AbilityTest` 启动链路中
- `AddBuffAction` 仍为空实现，是内容层未接通的直接缺口
- `1001_ComboGraph` 中存在 `Skill01` 节点，`2001_ComboGraph` 中存在 `HurtNormal / HurtDown / HurtFly` 节点

### 收敛后的 Demo 方案

- `Skill01 -> Bullet/3001 -> Buff/1001 -> 改写 AttackType -> Enemy HurtFly/HurtDown`
- 尽量复用现有 `Actor / Bullet / Buff / ComboGraph` 资源，不新建平行场景或平行系统

### 用户新增架构约束

- 实现思路必须尽量遵守文章内容，不能只做到功能跑通
- 第一原则：`流程骨架稳定，玩法逻辑外挂`
- 优先把玩法落在 `DamageInfo`、`AddBuffInfo`、`EffectComp` 分发、`BuffAction / BulletAction` 等扩展点中
- 避免把 Demo 特判直接硬编码进 `AttackComp`、`BulletManager`、`Main` 这类核心流程类里，除非只是极薄的调度或接线

### 子代理执行策略

- 已决定以新增约束为准，派发 `unity3d-ec-architect` 子代理任务
- 子代理提示明确禁止“为了快把 Demo 逻辑塞回核心流程类”的做法

### 测试方案修正

- 用户要求测试方案遵守项目内建议，已切换为 `tools/Run-HaloActEditModeTests.ps1`
- 已同步约束给 `unity3d-ec-architect` 子代理：禁止把 `dotnet test` 作为 Unity EditMode 主验证链路
- 后续验收统一要求：
  - 产物写入 `TestArtifacts/TestRunner`
  - batchmode 串行执行
  - 优先使用 `-TestClass` / `-TestMethod` / `-TestNamespace`
  - 不使用 `-executeMethod HaloFrame.Editor.HaloActEditModeBatchRunner.RunFromCommandLine`

### 本轮实现收口

- 新增 `BuffBulletDemoAssetSyncTests`，先让“Demo 资源已接线”这条 EditMode 测试红起来
- 红测暴露出 `AddBuffInfo` 的内容配置字段序列化不稳定，已把 `AddStack`、`Duration`、`Permanent` 改成显式字段
- 新增 Editor 侧 `BuffBulletDemoAssetSync`，把 `Skill01 -> Bullet/3001 -> Buff/1001` 资源链路一次性同步到位
- 已重新保存项目资源：
  - `Skill01.asset` 现在会发射 `3001`
  - `3001.asset` 命中后会挂 `1001`
  - `1001.asset` 受击时会把 `AttackType` 改成 `HitFly`
- 已通过项目建议脚本验证：
  - `tools/Run-HaloActEditModeTests.ps1 -TestClass Ability.Buff.BuffBulletDemoAssetSyncTests`
  - `tools/Run-HaloActEditModeTests.ps1 -TestNamespace Ability.Buff`
