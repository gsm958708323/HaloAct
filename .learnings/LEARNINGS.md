# Learnings

Corrections, insights, and knowledge gaps captured during development.

**Categories**: correction | insight | knowledge_gap | best_practice

---

## [LRN-20260405-002] best_practice

**Logged**: 2026-04-05T23:35:00+08:00
**Priority**: high
**Status**: pending
**Area**: tests

### Summary
Unity 项目的 EditMode/batchmode 测试默认不要创建 git worktree，优先直接在主工作区串行执行。

### Details
这次为隔离改动额外创建了 `.worktrees/ec-combat-full-stack`，但 Unity 在新 worktree 上需要重新导入工程、重建 Library，并且还遇到了本地插件 DLL 缺失、导入噪音文件增多、测试启动变慢等问题。对于本项目这类 Unity 2021 工程，测试的主要瓶颈在导入和编译，而不是源码隔离；把测试放在主干工作区串行执行，通常比新建 worktree 更省时、更稳。

### Suggested Action
后续遇到 Unity 项目测试任务时，默认直接在主工作区运行 `tools/Run-HaloActEditModeTests.ps1`，只有在用户明确要求隔离开发环境且能接受额外导入成本时，才考虑使用 worktree。

### Metadata
- Source: user_feedback
- Related Files: AGENTS.md, tools/Run-HaloActEditModeTests.ps1
- Tags: unity, tests, worktree, batchmode, performance
- Pattern-Key: best_practice.unity_tests_no_worktree
- Recurrence-Count: 1
- First-Seen: 2026-04-05
- Last-Seen: 2026-04-05

---

## [LRN-20260405-001] best_practice

**Logged**: 2026-04-05T09:19:41.3811635+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
Initialize self-improvement logging in the workspace root before using the skill.

### Details
The `self-improvement` skill expects a `.learnings/` directory in the project or workspace root. This workspace did not have the directory or the three default markdown log files, so initialization was required before recording any learning entries.

### Suggested Action
Keep `.learnings/` at the workspace root and append future learning, error, and feature request entries there.

### Metadata
- Source: conversation
- Related Files: .learnings/LEARNINGS.md
- Tags: self-improvement, workspace-setup, test-entry

---

## [LRN-20260405-002] best_practice

**Logged**: 2026-04-05T09:20:00+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
Prefer Git Bash absolute paths for Codex hook scripts on Windows when `bash` may resolve to WSL.

### Details
This machine has both Git for Windows and WSL available. Calling plain `bash` resolved to WSL first, which caused Windows path translation issues for the installed `self-improvement` shell scripts under `C:\Users\Halo\.codex\skills\...`. A small PowerShell wrapper that prefers Git Bash and only falls back to generic `bash` makes the hook setup more reliable.

### Suggested Action
Use PowerShell wrapper scripts in `.codex/` for Windows hook commands, and have them prefer Git Bash absolute paths before falling back to `bash`.

### Metadata
- Source: conversation
- Related Files: .codex/settings.json, .codex/self-improvement-activator.ps1, .codex/self-improvement-error-detector.ps1
- Tags: windows, git-bash, wsl, codex-hooks, self-improvement

---

## [LRN-20260405-003] best_practice

**Logged**: 2026-04-05T11:15:53.8249780+08:00
**Priority**: high
**Status**: pending
**Area**: tests

### Summary
Use `tools/Run-HaloActEditModeTests.ps1` as the canonical AI entrypoint for Unity batchmode EditMode tests in HaloAct.

### Details
Unity 2021.3 batchmode in this project is reliable when driven through the official `-runTests` command line and wrapped by `tools/Run-HaloActEditModeTests.ps1`. The script now normalizes the Unity path, writes outputs under `TestArtifacts/TestRunner`, blocks artifact paths under the project `Temp/` directory, and emits both XML and JSON summaries. By contrast, `-executeMethod HaloFrame.Editor.HaloActEditModeBatchRunner.RunFromCommandLine` is not reliable in batchmode because `TestRunnerApi` can exit before asynchronous callbacks complete.

### Suggested Action
Future agents should default to `tools/Run-HaloActEditModeTests.ps1` and select tests with `-TestClass`, `-TestMethod`, `-TestNamespace`, or raw `-TestFilter` instead of assembling raw Unity CLI invocations first.

### Metadata
- Source: conversation
- Related Files: tools/Run-HaloActEditModeTests.ps1, Assets/Scripts/HaloFrame/Editor/EditorTools/HaloActEditModeBatchRunner.cs
- Tags: unity, tests, batchmode, powershell, editmode
- Pattern-Key: harden.unity_test_entrypoint

---

## [LRN-20260405-004] best_practice

**Logged**: 2026-04-05T11:15:53.8249780+08:00
**Priority**: high
**Status**: pending
**Area**: tests

### Summary
In Unity 2021, HaloAct EditMode tests should compile into `Assembly-CSharp-Editor` unless the runtime code is also asmdef-based.

### Details
The earlier `HaloAct.EditModeTests.asmdef` approach caused test code to compile in a separate test assembly without an `Assembly-CSharp` reference. That meant the tests could not see runtime code still living in Unity's predefined assemblies. Moving the EditMode tests under `Assets/Tests/EditMode/Editor/` and removing the dedicated test asmdef restored visibility through `Assembly-CSharp-Editor` and made the runner usable again.

### Suggested Action
Keep EditMode tests under `Assets/Tests/EditMode/Editor/` by default. Only introduce a separate test asmdef after the runtime code under test has also been moved behind explicit asmdefs.

### Metadata
- Source: conversation
- Related Files: Assets/Tests/EditMode/Editor/, Assets/Tests/EditMode/HaloAct.EditModeTests.asmdef
- Tags: unity, asmdef, editmode, assembly-csharp-editor
- Pattern-Key: simplify.unity_test_assembly_layout

---

## [LRN-20260405-005] best_practice

**Logged**: 2026-04-05T23:38:00+08:00
**Priority**: high
**Status**: pending
**Area**: config

### Summary
可以使用子代理辅助生成代码，但不要使用代码评审子代理，主代理应专注一次性交付高质量实现。

### Details
用户明确要求后续允许使用子代理做代码实现类工作，但不需要代码评审子代理。关键设计判断、改动边界、集成验证和最终质量责任应由主代理自己承担，避免把关键把关工作外包给 review agent，导致 token 消耗增加、流程变长且收益有限。

### Suggested Action
后续只有在任务可明确拆分且确实能提速时，才创建实现类子代理；不要创建代码评审子代理。默认由主代理负责方案收敛、测试验证和最终交付质量。

### Metadata
- Source: user_feedback
- Related Files: AGENTS.md
- Tags: subagent, workflow, code-generation, quality
- Pattern-Key: best_practice.subagent_codegen_no_review_agent

---

## [LRN-20260405-006] best_practice

**Logged**: 2026-04-05T23:45:00+08:00
**Priority**: high
**Status**: pending
**Area**: backend

### Summary
给现有 Unity EC 战斗项目补 Buff、Damage、Bullet、AOE 时，优先采用“基础层先行，再做纵向扩展”的落地顺序。

### Details
这次实现里，先补 `AttrComp` 和稳定的 `EffectComp`，再引入 `DamageManager` 统一跨实体副作用，之后才接 Bullet、AOE 和配置校验。这个顺序明显降低了返工，因为属性层是 Buff 的落点，稳定 Buff 是统一伤害语义的前提，而统一伤害流水线又是 Bullet 和 AOE 共享行为的基础。直接从可见效果最强的 Bullet 或 AOE 开始，短期会更快出画面，但后面容易在回调顺序、叠加逻辑和配置语义上反复返工。

### Suggested Action
后续在类似项目里扩战斗系统时，默认先做运行时基础层，再逐个把上层玩法模块接进来，不要从最显眼的子系统倒推核心链路。

### Metadata
- Source: conversation
- Related Files: docs/plans/2026-04-05-ec-combat-full-stack-plan.md
- Tags: unity, ec, combat, architecture, sequencing
- Pattern-Key: best_practice.foundation_first_combat_stack

---

## [LRN-20260405-007] best_practice

**Logged**: 2026-04-05T23:45:00+08:00
**Priority**: high
**Status**: pending
**Area**: tests

### Summary
修战斗主链顺序时，先写跨子系统集成测试去锁死事件顺序，比只看局部单测更可靠。

### Details
这次真正暴露问题的不是 Buff、Bullet、AOE 各自的专项测试，而是一条集成测试：`Bullet -> Damage chain -> Buff applied -> Aoe reads new state`。局部单测都能过，但管理器优先级和同帧顺序仍然可能让 AOE 读到旧状态。只有把跨管理器顺序写成一条明确的时序断言，才能稳定验证“追加伤害先结算、延迟 Buff 后落地、同帧后续来源读取新状态”这种主链语义。

### Suggested Action
以后调整 `Manager.Tick()` 顺序、同帧事件链或延迟应用机制时，先补一条集成时序测试，再做实现改动。

### Metadata
- Source: conversation
- Related Files: Assets/Tests/EditMode/Editor/Ability/Integration/CombatIntegrationTests.cs
- Tags: unity, tests, integration, ordering, combat
- Pattern-Key: best_practice.integration_test_before_ordering_fix

---

## [LRN-20260405-008] best_practice

**Logged**: 2026-04-05T23:45:00+08:00
**Priority**: medium
**Status**: pending
**Area**: backend

### Summary
跨实体副作用应集中到统一 Manager，实体本地状态保留在组件内，不要为每个玩法再起一套平行框架。

### Details
这次最稳的映射方式不是照文章重新做一套 `BuffManager/BulletState/AoeState`，而是让实体本地状态继续放在 `EffectComp`、`BulletDataComp`、`AoeDataComp` 里，把真正跨实体的伤害、追加伤害、延迟 Buff 和主链顺序收拢到 `DamageManager`。这种做法既兼容现有 `Entity + Comp + Manager + ScriptableObject`，又能把顺序和副作用入口管住。如果把每个玩法都单独起一套管理体系，后面事件流和生命周期会越来越分裂。

### Suggested Action
后续扩展新的战斗子系统时，先判断它是“实体私有状态”还是“跨实体调度”，前者落组件，后者优先复用现有集中 Manager。

### Metadata
- Source: conversation
- Related Files: Assets/Scripts/Ability/Actor/EffectComp.cs, Assets/Scripts/Ability/Damage/DamageManager.cs, Assets/Scripts/Ability/Manager/BulletManager.cs, Assets/Scripts/Ability/Manager/AoeManager.cs
- Tags: unity, ec, manager, component, combat
- Pattern-Key: best_practice.local_state_plus_central_pipeline

---
