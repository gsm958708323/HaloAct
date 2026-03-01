# Assets/Scripts/Ability / AGENTS

## Overview
`Ability/` implements a frame-driven ability/combo system using ScriptableObject data:
- `AbilityNode` = transitions/conditions/priority
- `AbilityBehavior` (`BehaviorBase`) = action lists + input key + frame length
- `BehaviorComp` = runtime runner (loads assets, advances `curFrame`, switches nodes)

## Core Runtime Flow
- Entry: `BehaviorComp.Enter(IEntity model)` loads behavior + node assets from `PlayerDataComp.Data` paths.
- Behavior data: `Resources.LoadAll<AbilityBehavior>(behaviorPath)` then `behavior.Init()` + `AbilityAction.Init()` + `AbilityAttack.Init()`.
- Node data: `Resources.LoadAll<AbilityNode>(nodePath)` then `nodeList.Sort((x,y)=>x.Id.CompareTo(y.Id))`.
- Behavior binding: for each node, match `AbilityNode.name` to `AbilityBehavior.name` (also tries stripping digits: `Dash1/Dash2/Dash3` -> `Dash`).
- Start: `StartBehavior(GetBehaviorById(0))`.

## Frame Semantics
`BehaviorComp.curFrame` starts at `1` per behavior.
- `BehaviorBase.Tick(...)` calls `UpdateActions(tree.curFrame)`.
- `AbilityAction` runs on a closed frame interval:
  - `curFrame == StartFrame` -> `Enter(tree)`
  - `StartFrame <= curFrame <= EndFrame` -> `Tick(curFrame)`
  - `curFrame == EndFrame` -> `Exit()`
- When `curFrame > curNode.Behavior.FrameLength`:
  - If `IsLoop` -> `curFrame = 1`
  - Else -> `EndBehavior()` -> resets to node `0` and clears `Entity.Target`

## Node Transitions (Combos)
`BehaviorComp.TryGetNextBehavior()`:
- Iterates `curNode.Childs` and treats values as indices into `nodeList`.
- Checks input: `GameManager_Input.Instance.bufferKeys` contains `newNode.Behavior.InputKey`.
- Checks conditions: `AbilityNode.CheckCondition(...)`.
- Chooses the highest `AbilityNode.Priority` among valid candidates.

Gotcha: comments state “Index and Id are equal”; `Childs` must align with `nodeList` order after sorting by `Id`.

## Where To Look
| Task | Location |
|------|----------|
| Add a new node asset | `Assets/Scripts/Ability/AbilityNode.cs` (`CreateAssetMenu: AbilityTree/AbilityNode`) |
| Add a new behavior asset | `Assets/Scripts/Ability/Behavior/` (`CreateAssetMenu: AbilityTree/*Behavior`) |
| Add/modify action frame windows | `Assets/Scripts/Ability/AbilityAction.cs` + `Assets/Scripts/Ability/BehaviorBase.cs` |
| Add transition conditions | `Assets/Scripts/Ability/AbilityCondition.cs` + `Assets/Scripts/Ability/Condition/` |
| Attack windows and hitbox data | `Assets/Scripts/Ability/Behavior/AbilityBehaviorAttack.cs` + `Assets/Scripts/Ability/AbilityAttack.cs` |

## Anti-Patterns / Gotchas
- Don’t rename node/behavior assets casually: `BehaviorComp.LoadNode()` binds by asset `name` (with digit-stripping fallback).
- `GameManager_Input` and the new Input System can coexist; combo transitions currently read `bufferKeys` from `GameManager_Input`.
- `AbilityBehaviorAttack` has a `CreateAssetMenu` typo (`AbilityTree/Attackehavior`) — keep the exact menu path if tooling/scripts rely on it.
