# Errors

Command failures and integration errors.

---
## [ERR-20260405-001] rg.exe

**Logged**: 2026-04-05T10:09:55.6837610+08:00
**Priority**: medium
**Status**: pending
**Area**: tests

### Summary
`rg.exe` exists in the current PowerShell session but fails to execute with "Access is denied".

### Error
```
Program 'rg.exe' failed to run: Access is denied.
```

### Context
- Command attempted: `rg -n "com\\.unity\\.test-framework|com\\.unity\\.code-coverage" Packages/manifest.json Packages/packages-lock.json`
- Environment: Codex desktop app, PowerShell, Windows workspace at `D:\Work\UnityProject\HaloAct`
- Impact: Fallback to PowerShell native file search is required for local repo inspection.

### Suggested Fix
Check whether `rg.exe` is blocked by Windows permissions, AppLocker, or an invalid PATH target in this shell environment.

### Metadata
- Reproducible: unknown
- Related Files: Packages/manifest.json

---

## [ERR-20260405-002] unity_batchmode_project_lock

**Logged**: 2026-04-05T11:15:53.8249780+08:00
**Priority**: high
**Status**: pending
**Area**: tests

### Summary
Running multiple Unity batchmode test processes against the same project causes lock collisions and can escalate into a Unity crash.

### Error
```
Fatal Error! It looks like another Unity instance is running with this project open.
```

### Context
- Operation attempted: three `tools/Run-HaloActEditModeTests.ps1` runs launched in parallel against `D:\Work\UnityProject\HaloAct`
- Environment: Unity 2021.3.37f1, Windows, Codex desktop, batchmode EditMode tests
- Impact: no XML/JSON artifacts were written for the colliding runs, and Unity emitted a crash report under the local temp crash directory

### Suggested Fix
Serialize Unity test runs for this project. Before retrying, confirm there is no `Unity.exe` process holding the project and that `Temp/UnityLockfile` is gone.

### Metadata
- Reproducible: yes
- Related Files: tools/Run-HaloActEditModeTests.ps1
- See Also: LRN-20260405-003

---

## [ERR-20260405-214859] powershell-file-wsl-path

**Logged**: 2026-04-05T13:48:59.414833+00:00
**Priority**: medium
**Status**: pending
**Area**: tests

### Summary
Windows PowerShell `-File` invocation failed when given a WSL path to the Unity test runner script.

### Error
```
-File parameter cannot accept the WSL path form; convert the script path to a Windows path first.
```

### Context
- Command attempted to run `tools/Run-HaloActEditModeTests.ps1` via `powershell.exe -File` using `/mnt/d/...` path form.
- Environment is WSL/bash invoking Windows PowerShell.

### Suggested Fix
Use `wslpath -w` to convert the script path and any project paths passed into PowerShell/Unity commands.

### Metadata
- Reproducible: yes
- Related Files: tools/Run-HaloActEditModeTests.ps1

---
