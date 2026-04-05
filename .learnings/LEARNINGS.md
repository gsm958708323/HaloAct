# Learnings

Corrections, insights, and knowledge gaps captured during development.

**Categories**: correction | insight | knowledge_gap | best_practice

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
