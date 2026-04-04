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
        ObjectField actorField;
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
            actorField = new ObjectField("Actor")
            {
                objectType = typeof(ActorData),
                allowSceneObjects = false
            };
            actorField.RegisterValueChangedCallback(evt => LoadActor(evt.newValue as ActorData));
            toolbar.Add(actorField);

            toolbar.Add(new ToolbarButton(Reload) { text = "Reload" });
            toolbar.Add(new ToolbarButton(Save) { text = "Save" });
            toolbar.Add(new ToolbarButton(ValidateGraph) { text = "Validate" });
            toolbar.Add(new ToolbarButton(AutoLayout) { text = "Auto Layout" });
            toolbar.Add(new ToolbarButton(CreateNode) { text = "Create Node" });
            rootVisualElement.Add(toolbar);

            statusBox = new HelpBox("Select an ActorData asset to begin.", HelpBoxMessageType.Info);
            rootVisualElement.Add(statusBox);

            var split = new TwoPaneSplitView(0, 860, TwoPaneSplitViewOrientation.Horizontal);
            graphView = new ComboGraphView();
            inspectorPane = new ComboInspectorPane();
            split.Add(graphView);
            split.Add(inspectorPane);
            split.style.flexGrow = 1;
            rootVisualElement.Add(split);
        }

        void LoadActor(ActorData actor)
        {
            document = ComboEditorDocument.Load(actor);
            graphView.Bind(document, OnNodeSelected);
            inspectorPane.Bind(document, null, OnInspectorNodeChanged);
            UpdateStatus(actor);
        }

        void Reload()
        {
            AssetDatabase.Refresh();
            LoadActor(actorField?.value as ActorData);
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
            UpdateStatus(document.Actor);
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

            var node = CreateInstance<AbilityNode>();
            node.name = "NewNode";
            node.Id = GetNextNodeId();
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, $"Node_{node.Id}.asset").Replace("\\", "/"));
            AssetDatabase.CreateAsset(node, assetPath);
            AssetDatabase.SaveAssets();

            document.AddNode(node);
            graphView.Rebuild();
            inspectorPane.Bind(document, node, OnInspectorNodeChanged);
            UpdateStatus(document.Actor);
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
            UpdateStatus(document.Actor);
        }

        void UpdateStatus(ActorData actor)
        {
            if (statusBox == null)
            {
                return;
            }

            if (actor == null)
            {
                statusBox.messageType = HelpBoxMessageType.Info;
                statusBox.text = "Select an ActorData asset to begin.";
                return;
            }

            if (actor.ComboGraph == null)
            {
                statusBox.messageType = HelpBoxMessageType.Warning;
                statusBox.text = "No ComboGraph assigned.";
                return;
            }

            statusBox.messageType = document != null && document.IsDirty ? HelpBoxMessageType.Warning : HelpBoxMessageType.Info;
            statusBox.text = $"Loaded {actor.name}: {document?.Nodes.Count ?? 0} nodes, {actor.ComboGraph.LocalBehaviors.Count} behaviors.";
        }
    }
}
