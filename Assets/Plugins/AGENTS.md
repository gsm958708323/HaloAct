# Assets/Plugins / AGENTS

## Overview
`Assets/Plugins/` contains third-party vendor code. Treat it as read-only unless you are intentionally upgrading a dependency.

Major plugins present in this repo:
- `Assets/Plugins/Sirenix/` (Odin Inspector/Serializer)
- `Assets/Plugins/ParadoxNotion/` (NodeCanvas + CanvasCore)

## Rules
- Prefer extending in `Assets/Scripts/` instead of patching vendor sources.
- If you must patch vendor code:
  - Keep changes minimal and isolated.
  - Record the exact plugin version/source you upgraded from.
  - Expect deprecated APIs and editor workarounds (the vendor codebase contains `_DeprecatedFiles/` and `[Obsolete]` markers).

## Gotchas
- Vendor code may contain platform-specific workarounds and serialization hacks; review diffs carefully during upgrades.
