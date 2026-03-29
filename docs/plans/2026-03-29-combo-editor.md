# Combo Editor Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a GraphView-based combo editor that loads a character's full combo graph from the character resource, edits `AbilityNode` cards plus the selected node's `AbilityBehavior`, and saves explicit `Node.Behavior` references plus `Childs` node ids back into the character combo resource.

**Architecture:** Replace the current path-scan + name-match runtime loading with an explicit combo-root asset referenced by `ActorData`. The editor opens that combo-root asset, renders one GraphView card per `AbilityNode`, draws edges from `node.Childs`, edits node conditions on the node side, edits the selected node's `AbilityBehavior` on the right tab set, and saves by rebuilding node-child id lists plus direct behavior references.

**Tech Stack:** Unity 2021.3, C#, ScriptableObject assets, UIElements GraphView, Odin Inspector, Unity EditMode tests.

---

### Task 1: Introduce the explicit combo-root asset

**Files:**
- Create: `Assets/Scripts/Ability/Combo/ActorComboGraphSO.cs`
- Modify: `Assets/Scripts/Ability/Actor/ActorData.cs`
- Test: `Assets/Tests/EditMode/Ability/Combo/ActorComboGraphSOTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void ActorData_CanReferenceComboGraph()
{
    var actor = ScriptableObject.CreateInstance<ActorData>();
    var combo = ScriptableObject.CreateInstance<ActorComboGraphSO>();

    actor.ComboGraph = combo;

    Assert.That(actor.ComboGraph, Is.SameAs(combo));
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testFilter ActorData_CanReferenceComboGraph -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: FAIL because `ActorData.ComboGraph` and/or `ActorComboGraphSO` do not exist.

**Step 3: Write minimal implementation**

- Add `ActorComboGraphSO` as the single combo-root asset for a character.
- Give it explicit serialized references:
  - `List<AbilityNode> Nodes`
  - `List<AbilityBehavior> LocalBehaviors`
- Add helper methods:
  - `AbilityNode GetRootNode()`
  - `AbilityNode GetNodeById(int id)`
  - `IReadOnlyDictionary<int, AbilityNode> BuildNodeMap()`
- Add `public ActorComboGraphSO ComboGraph;` to `ActorData`.
- Keep `NodePath` and `BehaviorPath` temporarily for migration only; mark them as legacy in comments.

**Step 4: Run test to verify it passes**

Run the same `Unity.exe -runTests` command.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Ability/Combo/ActorComboGraphSO.cs Assets/Scripts/Ability/Actor/ActorData.cs Assets/Tests/EditMode/Ability/Combo/ActorComboGraphSOTests.cs
git commit -m "feat: add explicit actor combo graph asset"
```


### Task 2: Refactor runtime loading to use the combo-root asset

**Files:**
- Modify: `Assets/Scripts/Ability/Actor/BehaviorComp.cs`
- Modify: `Assets/Scripts/Ability/Actor/ActorData.cs`
- Test: `Assets/Tests/EditMode/Ability/Combo/BehaviorCompComboLoadTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void ComboGraphLoader_UsesNodeIdsForChildrenInsteadOfSortedIndexes()
{
    var root = ScriptableObject.CreateInstance<ActorComboGraphSO>();
    var idle = ScriptableObject.CreateInstance<AbilityNode>();
    var atk = ScriptableObject.CreateInstance<AbilityNode>();

    idle.Id = 0;
    idle.Childs = new List<int> { 7 };
    atk.Id = 7;
    root.Nodes = new List<AbilityNode> { idle, atk };

    var map = root.BuildNodeMap();

    Assert.That(map[0].Childs[0], Is.EqualTo(7));
    Assert.That(root.GetNodeById(7), Is.SameAs(atk));
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testFilter ComboGraphLoader_UsesNodeIdsForChildrenInsteadOfSortedIndexes -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: FAIL because runtime helpers still depend on `Resources.LoadAll` and index-based child lookup.

**Step 3: Write minimal implementation**

- Replace `LoadBehavior(data.BehaviorPath)` and `LoadNode(data.NodePath)` in `BehaviorComp.Enter()` with explicit loading from `data.ComboGraph`.
- Build a runtime node map by id and resolve children through `GetNodeById`.
- Delete name-based `Node -> Behavior` rebinding from `BehaviorComp.LoadNode`.
- Replace `GetBehaviorById(int id)` with `GetNodeById(int id)`.
- Update `TryGetNextBehavior()` to iterate `curNode.Childs` as node ids, not list indexes.
- Keep initialization behavior:
  - `behavior.Init()`
  - `AbilityAction.Init()`
  - `AbilityAttack.Init()`
  - start from node id `0`

**Step 4: Run focused and broad tests**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testFilter BehaviorCompComboLoadTests -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Ability/Actor/BehaviorComp.cs Assets/Scripts/Ability/Actor/ActorData.cs Assets/Tests/EditMode/Ability/Combo/BehaviorCompComboLoadTests.cs
git commit -m "refactor: load combo runtime from explicit graph asset"
```


### Task 3: Add combo graph validation and migration utilities

**Files:**
- Create: `Assets/Scripts/Ability/Combo/ComboGraphValidation.cs`
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphMigrationUtility.cs`
- Test: `Assets/Tests/EditMode/Ability/Combo/ComboGraphValidationTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void Validation_FlagsMissingBehaviorAndBrokenChildId()
{
    var node = ScriptableObject.CreateInstance<AbilityNode>();
    node.Id = 0;
    node.Childs = new List<int> { 99 };

    var result = ComboGraphValidation.Validate(new[] { node });

    Assert.That(result.Errors, Has.Some.Contains("Behavior"));
    Assert.That(result.Errors, Has.Some.Contains("99"));
}
```

**Step 2: Run test to verify it fails**

Run the same batchmode command with `-testFilter Validation_FlagsMissingBehaviorAndBrokenChildId`.

Expected: FAIL because validation service does not exist.

**Step 3: Write minimal implementation**

- Add validation rules:
  - root node id `0` exists
  - node ids are unique
  - all `Childs` values resolve to a node id
  - every node has a non-null `Behavior`
  - every `Behavior` referenced by a node exists in either `LocalBehaviors` or as an external shared asset
- Add migration utility that:
  - reads legacy `NodePath` / `BehaviorPath`
  - creates `ActorComboGraphSO`
  - fills `Nodes` and `LocalBehaviors`
  - assigns `ActorData.ComboGraph`
  - saves assets without changing runtime behavior semantics

**Step 4: Run tests**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testFilter ComboGraphValidationTests -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Ability/Combo/ComboGraphValidation.cs Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphMigrationUtility.cs Assets/Tests/EditMode/Ability/Combo/ComboGraphValidationTests.cs
git commit -m "feat: add combo graph validation and migration utility"
```


### Task 4: Create the editor window shell and open a character combo graph

**Files:**
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs`
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorDocument.cs`
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorSelection.cs`
- Test: `Assets/Tests/EditMode/Ability/Combo/ComboEditorWindowTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void Window_OpenForActor_LoadsComboGraphDocument()
{
    var actor = ScriptableObject.CreateInstance<ActorData>();
    actor.ComboGraph = ScriptableObject.CreateInstance<ActorComboGraphSO>();

    var document = ComboEditorDocument.FromActor(actor);

    Assert.That(document.Actor, Is.SameAs(actor));
    Assert.That(document.ComboGraph, Is.SameAs(actor.ComboGraph));
}
```

**Step 2: Run test to verify it fails**

Run the batchmode command with `-testFilter Window_OpenForActor_LoadsComboGraphDocument`.

Expected: FAIL because the document/window types do not exist.

**Step 3: Write minimal implementation**

- Add menu entry like `Tools/Ability/连招编辑器`.
- Build an editor document around:
  - selected `ActorData`
  - loaded `ActorComboGraphSO`
  - current graph node view models
  - dirty state
- Add toolbar commands:
  - select actor
  - reload
  - validate
  - save
  - auto layout
  - create node

**Step 4: Run tests**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testFilter ComboEditorWindowTests -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorDocument.cs Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorSelection.cs Assets/Tests/EditMode/Ability/Combo/ComboEditorWindowTests.cs
git commit -m "feat: add combo editor window shell"
```


### Task 5: Implement the GraphView node cards and child-id edges

**Files:**
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphView.cs`
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboNodeView.cs`
- Test: `Assets/Tests/EditMode/Ability/Combo/ComboGraphViewTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void GraphView_BuildsEdgeFromNodeChildIds()
{
    var root = ScriptableObject.CreateInstance<AbilityNode>();
    var child = ScriptableObject.CreateInstance<AbilityNode>();
    root.Id = 0;
    child.Id = 3;
    root.Childs = new List<int> { 3 };

    var graph = new ComboGraphView();
    graph.Load(new[] { root, child });

    Assert.That(graph.edges.ToList(), Has.Count.EqualTo(1));
}
```

**Step 2: Run test to verify it fails**

Run the batchmode command with `-testFilter GraphView_BuildsEdgeFromNodeChildIds`.

Expected: FAIL because the GraphView types do not exist.

**Step 3: Write minimal implementation**

- Build one card per `AbilityNode`.
- Node card displays:
  - node name
  - id
  - priority
  - conditions summary
  - read-only behavior summary
- Rebuild edges only from `node.Childs`.
- On edge create:
  - add target node id to source node `Childs`
- On edge delete:
  - remove target node id from source node `Childs`
- Never store edge-only data.

**Step 4: Run tests**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testFilter ComboGraphViewTests -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphView.cs Assets/Scripts/Ability/Editor/ComboEditor/ComboNodeView.cs Assets/Tests/EditMode/Ability/Combo/ComboGraphViewTests.cs
git commit -m "feat: render combo graph nodes and child-id edges"
```


### Task 6: Add right-side Node and Behavior tabs

**Files:**
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboInspectorPane.cs`
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboBehaviorInspector.cs`
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboNodeInspector.cs`
- Test: `Assets/Tests/EditMode/Ability/Combo/ComboInspectorPaneTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void Inspector_SelectingNodeShowsBehaviorTabBoundToNodeBehavior()
{
    var behavior = ScriptableObject.CreateInstance<AbilityBehaviorRoot>();
    var node = ScriptableObject.CreateInstance<AbilityNode>();
    node.Behavior = behavior;

    var pane = new ComboInspectorPane();
    pane.Bind(node);

    Assert.That(pane.SelectedBehavior, Is.SameAs(behavior));
}
```

**Step 2: Run test to verify it fails**

Run the batchmode command with `-testFilter Inspector_SelectingNodeShowsBehaviorTabBoundToNodeBehavior`.

Expected: FAIL because the inspector pane does not exist.

**Step 3: Write minimal implementation**

- Add two tabs:
  - `Behavior`
  - `Node`
- `Behavior` tab:
  - full editable inspector for selected node's `AbilityBehavior`
  - shared/local source badge
  - reference usage summary
- `Node` tab:
  - editable `Id`
  - editable `Priority`
  - editable `conditions`
  - read-only `Childs` summary
- Changing `Id` triggers graph refresh and validation.

**Step 4: Run tests**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testFilter ComboInspectorPaneTests -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Ability/Editor/ComboEditor/ComboInspectorPane.cs Assets/Scripts/Ability/Editor/ComboEditor/ComboBehaviorInspector.cs Assets/Scripts/Ability/Editor/ComboEditor/ComboNodeInspector.cs Assets/Tests/EditMode/Ability/Combo/ComboInspectorPaneTests.cs
git commit -m "feat: add behavior and node inspector tabs"
```


### Task 7: Implement authoritative save from the graph back into assets

**Files:**
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphSaveService.cs`
- Modify: `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs`
- Test: `Assets/Tests/EditMode/Ability/Combo/ComboGraphSaveServiceTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void Save_RewritesNodeBehaviorAndChildIdsFromGraphState()
{
    var behavior = ScriptableObject.CreateInstance<AbilityBehaviorRoot>();
    var a = ScriptableObject.CreateInstance<AbilityNode>();
    var b = ScriptableObject.CreateInstance<AbilityNode>();
    a.Id = 0;
    b.Id = 9;

    var document = ComboEditorDocument.ForTests(a, b);
    document.BindBehavior(a, behavior);
    document.Connect(a, b);

    ComboGraphSaveService.Save(document);

    Assert.That(a.Behavior, Is.SameAs(behavior));
    Assert.That(a.Childs, Is.EqualTo(new[] { 9 }));
}
```

**Step 2: Run test to verify it fails**

Run the batchmode command with `-testFilter Save_RewritesNodeBehaviorAndChildIdsFromGraphState`.

Expected: FAIL because save service does not exist.

**Step 3: Write minimal implementation**

- On save:
  - validate the current document
  - write each node's `Behavior`
  - rebuild each node's `Childs` from outgoing edges using target node ids
  - normalize combo graph `Nodes` and `LocalBehaviors`
  - mark dirty and save all touched assets
- Save target assets:
  - the current `ActorComboGraphSO`
  - touched `AbilityNode` assets
  - touched local/shared `AbilityBehavior` assets
  - the owning `ActorData`
- Show an error dialog if validation fails; do not partially save.

**Step 4: Run tests**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testFilter ComboGraphSaveServiceTests -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Ability/Editor/ComboEditor/ComboGraphSaveService.cs Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorWindow.cs Assets/Tests/EditMode/Ability/Combo/ComboGraphSaveServiceTests.cs
git commit -m "feat: save combo graph edges and behavior bindings"
```


### Task 8: Seed the existing actor assets and run end-to-end verification

**Files:**
- Modify: `Assets/Scripts/Ability/Resources/Actor/1001.asset`
- Modify: `Assets/Scripts/Ability/Resources/Actor/2001.asset`
- Create: `Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorManualChecklist.md`
- Test: `Assets/Tests/EditMode/Ability/Combo/ComboMigrationSmokeTests.cs`

**Step 1: Write the failing smoke test**

```csharp
[Test]
public void ExistingActor1001_HasComboGraphAssigned()
{
    var actor = AssetDatabase.LoadAssetAtPath<ActorData>(
        "Assets/Scripts/Ability/Resources/Actor/1001.asset");

    Assert.That(actor.ComboGraph, Is.Not.Null);
}
```

**Step 2: Run test to verify it fails**

Run the batchmode command with `-testFilter ExistingActor1001_HasComboGraphAssigned`.

Expected: FAIL before migration is applied.

**Step 3: Implement migration and asset updates**

- Use the migration utility to create combo graph assets for:
  - `1001`
  - `2001`
- Assign those combo graph assets to the corresponding `ActorData`.
- Open the new editor for `1001` and verify manually:
  - node cards load
  - edges match `Childs`
  - selecting a node opens the `Behavior` tab
  - editing a behavior field marks the document dirty
  - save rewrites `Node.Behavior` and `Childs`
- Record the manual smoke checklist in `ComboEditorManualChecklist.md`.

**Step 4: Run the full editmode suite**

Run:

```bash
Unity.exe -batchmode -projectPath "C:\Users\Halo\.codex\worktrees\5670\HaloAct" -runTests -testPlatform editmode -testResults "C:\Users\Halo\.codex\worktrees\5670\HaloAct\Temp\combo-editor-tests.xml"
```

Expected: PASS and updated actor assets checked in.

**Step 5: Commit**

```bash
git add Assets/Scripts/Ability/Resources/Actor/1001.asset Assets/Scripts/Ability/Resources/Actor/2001.asset Assets/Scripts/Ability/Editor/ComboEditor/ComboEditorManualChecklist.md Assets/Tests/EditMode/Ability/Combo/ComboMigrationSmokeTests.cs
git commit -m "feat: wire actors to combo graph assets"
```


## Non-Goals for This Plan

- Do not implement the frame-timeline editor yet.
- Do not redesign the runtime action system beyond the combo-root load/save refactor.
- Do not move `conditions` off `AbilityNode`.
- Do not create edge-owned transition assets in this phase.

## Verification Notes

- Prefer EditMode tests for all save/load/validation behavior.
- Manual verification is required for GraphView interactions because GraphView UI is awkward to fully automate in this project.
- Before claiming completion, verify:
  - `ActorData -> ComboGraph` loads for `1001`
  - runtime no longer depends on `Resources.LoadAll` or name-based behavior matching
  - save rewrites `Node.Behavior` and `Childs` from the graph state
  - the editor clearly distinguishes `Node` data from `Behavior` editing tabs

