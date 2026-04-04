using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ability.Editor.Combo
{
    public class ComboEditorWindow : EditorWindow
    {
        ComboEditorDocument document;
        ComboGraphView graphView;
        ComboInspectorPane inspectorPane;
        ObjectField graphField;
        HelpBox statusBox;

        [MenuItem("Tools/Ability/Combo Editor")]
        static void Open()
        {
            var window = GetWindow<ComboEditorWindow>();
            window.titleContent = new GUIContent("Combo Editor");
            window.minSize = new Vector2(1200, 700);
            window.Show();
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
            graphView.Bind(document, OnNodeSelected, OnNodeCardChanged);
            inspectorPane.Bind(document, null, OnInspectorNodeChanged);
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
            comboGraph.name = Path.GetFileNameWithoutExtension(assetPath);
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
            if (document?.ComboGraph == null)
            {
                EditorUtility.DisplayDialog("Create Node", "Load a combo graph first.", "OK");
                return;
            }

            var graphPath = AssetDatabase.GetAssetPath(document.ComboGraph);
            var directory = Path.GetDirectoryName(graphPath);
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            var nextNodeId = GetNextNodeId();
            var node = CreateInstance<AbilityNode>();
            node.Id = nextNodeId;
            node.name = $"Node_{nextNodeId}";

            var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, $"{node.name}.asset").Replace("\\", "/"));
            AssetDatabase.CreateAsset(node, assetPath);
            AssetDatabase.SaveAssets();

            document.AddNode(node);
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
            inspectorPane.Bind(document, node, OnInspectorNodeChanged);
            UpdateStatus(document.ComboGraph);
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
                return Path.GetDirectoryName(AssetDatabase.GetAssetPath(document.ComboGraph))?.Replace("\\", "/");
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

                    return Path.GetDirectoryName(selectedPath)?.Replace("\\", "/");
                }
            }

            return "Assets";
        }
    }
}
