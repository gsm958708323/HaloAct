using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ability.Editor.Combo
{
    public class ComboEditorWindow : EditorWindow
    {
        const double RuntimeHighlightSyncInterval = 0.1d;

        ComboEditorDocument document;
        ComboGraphView graphView;
        ComboInspectorPane inspectorPane;
        ObjectField graphField;
        HelpBox statusBox;
        readonly Dictionary<AbilityNode, int> runtimeNodeCounts = new();
        double nextRuntimeHighlightSyncTime;

        [MenuItem("Tools/Ability/Combo Editor")]
        static void Open()
        {
            var window = GetWindow<ComboEditorWindow>();
            window.titleContent = new GUIContent("Combo Editor");
            window.minSize = new Vector2(1200, 700);
            window.Show();
        }

        void OnEnable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        void CreateGUI()
        {
            rootVisualElement.Clear();

            var toolbar = new Toolbar();
            graphField = new ObjectField("Combo Graph")
            {
                objectType = typeof(ActorComboGraphSO),
                allowSceneObjects = false
            };
            graphField.RegisterValueChangedCallback(evt => LoadGraph(evt.newValue as ActorComboGraphSO));
            toolbar.Add(graphField);

            toolbar.Add(new ToolbarButton(CreateGraph) { text = "New Graph" });
            toolbar.Add(new ToolbarButton(Reload) { text = "Reload" });
            toolbar.Add(new ToolbarButton(Save) { text = "Save" });
            toolbar.Add(new ToolbarButton(ValidateGraph) { text = "Validate" });
            toolbar.Add(new ToolbarButton(AutoLayout) { text = "Auto Layout" });
            toolbar.Add(new ToolbarButton(CreateNode) { text = "Create Node" });
            rootVisualElement.Add(toolbar);

            statusBox = new HelpBox("Select an ActorComboGraphSO asset to begin.", HelpBoxMessageType.Info);
            rootVisualElement.Add(statusBox);

            var split = new TwoPaneSplitView(0, 860, TwoPaneSplitViewOrientation.Horizontal);
            graphView = new ComboGraphView();
            inspectorPane = new ComboInspectorPane();
            split.Add(graphView);
            split.Add(inspectorPane);
            split.style.flexGrow = 1;
            rootVisualElement.Add(split);

            if (Selection.activeObject is ActorComboGraphSO comboGraph)
            {
                graphField.SetValueWithoutNotify(comboGraph);
                LoadGraph(comboGraph);
            }
        }

        void LoadGraph(ActorComboGraphSO comboGraph)
        {
            document = ComboEditorDocument.Load(comboGraph);
            graphView.Bind(document, OnNodeSelected, OnNodeCardChanged, CreateNodeAt);
            inspectorPane.Bind(document, null, OnInspectorNodeChanged);
            RefreshRuntimeHighlights(true);
            UpdateStatus(comboGraph);
        }

        void Reload()
        {
            AssetDatabase.Refresh();
            LoadGraph(graphField?.value as ActorComboGraphSO);
        }

        void CreateGraph()
        {
            var assetPath = EditorUtility.SaveFilePanelInProject(
                "Create Combo Graph",
                "NewComboGraph",
                "asset",
                "Choose where to create the combo graph asset.",
                GetInitialAssetDirectory());

            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            var comboGraph = CreateInstance<ActorComboGraphSO>();
            comboGraph.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(comboGraph, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            graphField.SetValueWithoutNotify(comboGraph);
            LoadGraph(comboGraph);
            EditorGUIUtility.PingObject(comboGraph);
        }

        void Save()
        {
            if (document == null)
            {
                return;
            }

            if (document.ComboGraph == null)
            {
                EditorUtility.DisplayDialog("Combo Save Failed", "No combo graph is assigned.", "OK");
                return;
            }

            if (!ComboGraphSaveService.Save(document, out var validation))
            {
                EditorUtility.DisplayDialog("Combo Save Failed", string.Join("\n", validation.Errors), "OK");
                return;
            }

            graphView.Rebuild();
            RefreshRuntimeHighlights(true);
            inspectorPane.Refresh();
            UpdateStatus(document.ComboGraph);
        }

        void ValidateGraph()
        {
            if (document?.ComboGraph == null)
            {
                EditorUtility.DisplayDialog("Combo Validation", "No combo graph loaded.", "OK");
                return;
            }

            var validation = ComboGraphValidation.Validate(document.ComboGraph);
            var message = validation.IsValid ? "Combo graph is valid." : string.Join("\n", validation.Errors);
            EditorUtility.DisplayDialog("Combo Validation", message, "OK");
        }

        void AutoLayout()
        {
            graphView?.AutoLayout();
        }

        void CreateNode()
        {
            var fallbackPosition = ComboGraphLayout.GetDefaultPosition(document?.Nodes.Count ?? 0).position;
            CreateNodeAt(fallbackPosition);
        }

        void CreateNodeAt(Vector2 position)
        {
            if (document?.ComboGraph == null)
            {
                EditorUtility.DisplayDialog("Create Node", "Load a combo graph first.", "OK");
                return;
            }

            var graphPath = AssetDatabase.GetAssetPath(document.ComboGraph);
            if (string.IsNullOrEmpty(graphPath))
            {
                return;
            }

            var nextNodeId = GetNextNodeId();
            var node = CreateInstance<AbilityNode>();
            node.Id = nextNodeId;
            node.name = $"Node_{nextNodeId}";
            AssetDatabase.AddObjectToAsset(node, document.ComboGraph);
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(document.ComboGraph);
            AssetDatabase.ImportAsset(graphPath);
            AssetDatabase.SaveAssets();

            document.AddNode(node, ComboGraphLayout.GetPositionAt(position));
            graphView.Rebuild();
            graphView.SelectNode(node);
            inspectorPane.Bind(document, node, OnInspectorNodeChanged);
            UpdateStatus(document.ComboGraph);
            EditorGUIUtility.PingObject(node);
        }

        int GetNextNodeId()
        {
            if (document == null || document.Nodes.Count == 0)
            {
                return 0;
            }

            var maxId = -1;
            for (int i = 0; i < document.Nodes.Count; i++)
            {
                if (document.Nodes[i] != null && document.Nodes[i].Id > maxId)
                {
                    maxId = document.Nodes[i].Id;
                }
            }

            return maxId + 1;
        }

        void OnNodeSelected(AbilityNode node)
        {
            inspectorPane.Bind(document, node, OnInspectorNodeChanged);
        }

        void OnInspectorNodeChanged(AbilityNode node)
        {
            if (document == null)
            {
                return;
            }

            document.MarkDirty();
            graphView.Rebuild();
            RefreshRuntimeHighlights(true);
            inspectorPane.Refresh();
            UpdateStatus(document.ComboGraph);
        }

        void OnNodeCardChanged(AbilityNode node)
        {
            if (document == null || node == null)
            {
                return;
            }

            graphView.RefreshNode(node);
            RefreshRuntimeHighlights(true);
            inspectorPane.Bind(document, node, OnInspectorNodeChanged);
            UpdateStatus(document.ComboGraph);
        }

        void OnEditorUpdate()
        {
            if (graphView == null)
            {
                return;
            }

            RefreshRuntimeHighlights(false);
        }

        void UpdateStatus(ActorComboGraphSO comboGraph)
        {
            if (statusBox == null)
            {
                return;
            }

            if (comboGraph == null)
            {
                statusBox.messageType = HelpBoxMessageType.Info;
                statusBox.text = "Select an ActorComboGraphSO asset to begin.";
                return;
            }

            statusBox.messageType = document != null && document.IsDirty ? HelpBoxMessageType.Warning : HelpBoxMessageType.Info;
            statusBox.text = $"Loaded {comboGraph.name}: {document?.Nodes.Count ?? 0} nodes, {document?.GetLocalBehaviors().Count ?? 0} local behaviors.";
        }

        string GetInitialAssetDirectory()
        {
            if (document?.ComboGraph != null)
            {
                var comboGraphPath = AssetDatabase.GetAssetPath(document.ComboGraph);
                return System.IO.Path.GetDirectoryName(comboGraphPath)?.Replace("\\", "/");
            }

            if (Selection.activeObject != null)
            {
                var selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (AssetDatabase.IsValidFolder(selectedPath))
                    {
                        return selectedPath;
                    }

                    return System.IO.Path.GetDirectoryName(selectedPath)?.Replace("\\", "/");
                }
            }

            return "Assets";
        }

        void RefreshRuntimeHighlights(bool force)
        {
            if (graphView == null)
            {
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                if (force || runtimeNodeCounts.Count > 0)
                {
                    runtimeNodeCounts.Clear();
                    graphView.SetRuntimeNodeCounts(runtimeNodeCounts);
                }

                return;
            }

            if (!force && EditorApplication.timeSinceStartup < nextRuntimeHighlightSyncTime)
            {
                return;
            }

            nextRuntimeHighlightSyncTime = EditorApplication.timeSinceStartup + RuntimeHighlightSyncInterval;
            var latestCounts = CollectRuntimeNodeCounts();
            if (!force && AreRuntimeNodeCountsEqual(runtimeNodeCounts, latestCounts))
            {
                return;
            }

            runtimeNodeCounts.Clear();
            foreach (var pair in latestCounts)
            {
                runtimeNodeCounts[pair.Key] = pair.Value;
            }

            graphView.SetRuntimeNodeCounts(runtimeNodeCounts);
        }

        Dictionary<AbilityNode, int> CollectRuntimeNodeCounts()
        {
            var activeCounts = new Dictionary<AbilityNode, int>();
            if (document?.ComboGraph == null || FightManager.LogicEntity == null)
            {
                return activeCounts;
            }

            var actorList = FightManager.LogicEntity.GetEntityLinkedList(EntityType.Actor);
            if (actorList == null)
            {
                return activeCounts;
            }

            for (var node = actorList.First; node != null; node = node.Next)
            {
                if (node.Value is not Entity entity)
                {
                    continue;
                }

                var dataComp = entity.GetComp<PlayerDataComp>();
                if (dataComp?.Data?.ComboGraph != document.ComboGraph)
                {
                    continue;
                }

                var behaviorComp = entity.GetComp<BehaviorComp>();
                if (behaviorComp?.curNode == null)
                {
                    continue;
                }

                if (!activeCounts.ContainsKey(behaviorComp.curNode))
                {
                    activeCounts.Add(behaviorComp.curNode, 1);
                    continue;
                }

                activeCounts[behaviorComp.curNode] += 1;
            }

            return activeCounts;
        }

        static bool AreRuntimeNodeCountsEqual(IReadOnlyDictionary<AbilityNode, int> left, IReadOnlyDictionary<AbilityNode, int> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Count != right.Count)
            {
                return false;
            }

            foreach (var pair in left)
            {
                if (!right.TryGetValue(pair.Key, out var value) || value != pair.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
