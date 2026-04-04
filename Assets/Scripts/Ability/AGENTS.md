# Assets/Scripts/Ability / AGENTS

## Overview
`Ability/` is a frame-driven combo system built around four core concepts:

- `ActorData.ComboGraph`
  Runtime entry for a character's combo set.
- `ActorComboGraphSO`
  The combo graph asset. It owns the node set and references local/shared behaviors.
- `AbilityNode`
  A combo node. Defines transition targets, conditions, priority, bound behavior, and editor layout state.
- `AbilityBehavior` / `BehaviorBase`
  The executable segment. Defines frame length, input key, loop flag, actions, and attack windows.

The runtime no longer loads nodes/behaviors from `Resources` paths or binds by asset name.
The source of truth is now:

`ActorData -> ComboGraph -> Nodes/Behaviors`

## Current Runtime Model
### Entry
- File: `Assets/Scripts/Ability/Actor/BehaviorComp.cs`
- `BehaviorComp.Enter(IEntity model)` reads `model.GetComp<PlayerDataComp>().Data.ComboGraph`
- If `ComboGraph` is missing, runtime logs an error and does not initialize combos

### Load Flow
- `BehaviorComp.LoadComboGraph(ActorComboGraphSO comboGraph)`
- Builds `nodeList` and `nodeDict` directly from `comboGraph.Nodes`
- Sorts `nodeList` by `AbilityNode.Id`
- Collects behaviors from:
  - every `node.Behavior`
  - `comboGraph.LocalBehaviors`
- Calls `behavior.Init()`
- Calls `AbilityAction.Init()` for every action
- Calls `AbilityAttack.Init()` for attack behaviors
- Records hurt nodes when `node.Behavior is AbilityBehaviorHurt`

### Tick Flow
- Fixed-step driver is still the HaloFrame manager loop at 15 FPS
- `BehaviorComp.Tick(float deltaTime)` order:
  1. Try transition to next node
  2. Tick current node with `curFrame`
  3. Increment `curFrame`
  4. End or loop behavior when `curFrame > FrameLength`

### Transition Logic
- File: `Assets/Scripts/Ability/Actor/BehaviorComp.cs`
- `AbilityNode.Childs` stores target node **Ids**, not list indices
- `TryGetNextBehavior()`:
  - iterates current node's `Childs`
  - resolves target node by `GetNodeById(childId)`
  - checks whether `GameManager_Input.Instance.bufferKeys` contains `target.Behavior.InputKey`
  - checks `target.CheckCondition(this)`
  - chooses the highest `target.Priority`

### Data Ownership
- `AbilityNode.conditions` lives on the node
- `AbilityBehavior` is executable data and may be reused by multiple nodes
- `AbilityNode` is now intended to be embedded under its owning `ActorComboGraphSO`
- `AbilityBehavior` may be:
  - local to a graph
  - external/shared asset

## Important Asset Types
### `AbilityNode`
- File: `Assets/Scripts/Ability/AbilityNode.cs`
- Fields that matter at runtime:
  - `Id`
  - `Childs`
  - `conditions`
  - `Priority`
  - `Behavior`
- Fields used by editor only:
  - `EditorPosition`
  - `EditorRect`
  - `HasEditorPosition`

### `ActorComboGraphSO`
- File: `Assets/Scripts/Ability/Combo/ActorComboGraphSO.cs`
- Owns:
  - `List<AbilityNode> Nodes`
  - `List<AbilityBehavior> LocalBehaviors`
- Utility methods:
  - `GetRootNode()`
  - `GetNodeById(int id)`
  - `BuildNodeMap()`

### `AbilityBehavior`
- File: `Assets/Scripts/Ability/Behavior/AbilityBehavior.cs`
- Extends `BehaviorBase`
- Core fields:
  - `FrameLength`
  - `Actions`
  - `IsLoop`
  - `InputKey`

### `BehaviorBase`
- File: `Assets/Scripts/Ability/BehaviorBase.cs`
- Executes actions by frame window:
  - `curFrame == StartFrame` -> `Enter(tree)`
  - `StartFrame <= curFrame <= EndFrame` -> `Tick(curFrame)`
  - `curFrame == EndFrame` -> `Exit()`

## Combo Editor
### Entry
- File: `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs`
- Menu: `Tools/Ability/Combo Editor`
- The editor opens **`ActorComboGraphSO` directly**
- It does **not** open `ActorData`

### Main Components
- `ComboEditorWindow`
  Main window, toolbar, graph loading, save, validate, auto layout, create graph, create node
- `ComboEditorDocument`
  In-memory editing model for nodes, edges, positions, local behaviors, and removed nodes
- `ComboGraphView`
  GraphView canvas, node visuals, edge creation/removal, right-click create node
- `ComboNodeView`
  Per-node card UI
- `ComboInspectorPane`
  Right-side inspector tabs for selected node
- `ComboGraphSaveService`
  Applies graph state back into assets and saves them
- `ComboGraphValidation`
  Validates graph correctness before save

### Node Card Behavior
- File: `Assets/Scripts/Ability/Editor/ComboEditor/ComboNodeView.cs`
- Each card represents one `AbilityNode`
- Card supports direct editing of:
  - `Priority`
  - `Conditions`
  - `Behavior`
- Card also supports:
  - `New` to create a local behavior asset
  - `Ping` to locate current behavior

### Right Inspector
- File: `Assets/Scripts/Ability/Editor/ComboEditor/ComboInspectorPane.cs`
- Tabs:
  - `Behavior`
  - `Node`
- `Behavior` tab can:
  - rebind the behavior
  - create a local behavior
  - edit the bound behavior inline
- `Node` tab edits the node asset directly

### Create Node
- File: `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs`
- Supported from:
  - toolbar `Create Node`
  - graph right-click context menu `Create Node`
- New nodes are created as **sub-assets of the current `ActorComboGraphSO`**
- New node id is `max(existing Id) + 1`

### Layout / Position Persistence
- Layout constants live in `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphLayout.cs`
- `Auto Layout` uses wider spacing than older versions
- Node positions are stored into the node asset itself:
  - `EditorRect`
  - `EditorPosition`
  - `HasEditorPosition`
- After reopen, `ComboEditorDocument` restores node positions from those fields

### Save Semantics
- File: `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphSaveService.cs`
- On save:
  - writes node positions back to `AbilityNode`
  - rewrites `node.Childs` from graph edges using target node ids
  - rewrites `comboGraph.Nodes`
  - rewrites `comboGraph.LocalBehaviors`
  - validates the graph
  - destroys removed node sub-assets

This means deleting a node in GraphView is not complete until the user clicks `Save`.

### Validation Rules
- File: `Assets/Scripts/Ability/Combo/ComboGraphValidation.cs`
- Current validation checks:
  - graph exists
  - graph has at least one node
  - root node id `0` exists
  - node ids are unique
  - every node has a behavior
  - every child id points to an existing node

## Migration / Legacy Notes
- Old runtime assumptions based on `Resources.LoadAll`, `NodePath`, `BehaviorPath`, or name-based node-behavior matching are obsolete
- Existing old graphs can be normalized with:
  - `Tools/Ability/Embed Combo Graph Nodes`
- File: `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphNodeSubAssetUtility.cs`
- Purpose:
  - ensure nodes referenced by a graph are embedded as graph sub-assets
  - normalize and sort graph node list

## High-Value Files
| Task | File |
|------|------|
| Runtime combo entry | `Assets/Scripts/Ability/Actor/BehaviorComp.cs` |
| Graph asset structure | `Assets/Scripts/Ability/Combo/ActorComboGraphSO.cs` |
| Node data | `Assets/Scripts/Ability/AbilityNode.cs` |
| Behavior data | `Assets/Scripts/Ability/Behavior/AbilityBehavior.cs` |
| Frame window execution | `Assets/Scripts/Ability/BehaviorBase.cs` |
| Editor window | `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs` |
| Graph canvas | `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphView.cs` |
| Node card UI | `Assets/Scripts/Ability/Editor/ComboEditor/ComboNodeView.cs` |
| Node/behavior editing helpers | `Assets/Scripts/Ability/Editor/ComboEditor/ComboNodeEditorUtility.cs` |
| Save/apply pipeline | `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphSaveService.cs` |
| Validation | `Assets/Scripts/Ability/Combo/ComboGraphValidation.cs` |

## Gotchas
- `AbilityNode.Childs` is a list of **Ids**, not indices
- `AbilityNode` should be treated as graph-owned data, not shared gameplay assets
- `AbilityBehavior` may be shared; editing a shared behavior affects every node that references it
- Combo transitions still read `GameManager_Input.Instance.bufferKeys`; this is separate from the new Input System setup used elsewhere
- If a node is deleted in the editor and comes back after reopen, first inspect whether the graph was actually saved
- If graph validation fails, save is aborted and asset changes are not finalized

## Recommended Mental Model
When changing combo gameplay, think in this order:

1. `ActorData` chooses which combo graph a character uses
2. `ActorComboGraphSO` owns the graph
3. `AbilityNode` decides where the combo can go next
4. `AbilityBehavior` decides what the current segment does
5. `AbilityAction` and attack data decide what happens on each frame

When changing the editor, think in this order:

1. `ComboEditorDocument` is the editing source of truth
2. `ComboGraphView` edits topology and position
3. `ComboNodeView` edits lightweight node fields
4. `ComboInspectorPane` edits deep node/behavior details
5. `ComboGraphSaveService` is the only place that should finalize asset state
