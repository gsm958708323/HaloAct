using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using EditorType = UnityEditor.Editor;

namespace Ability.Editor.Combo
{
    public class ComboInspectorPane : VisualElement
    {
        enum InspectorTab
        {
            Behavior,
            Node,
        }

        readonly Label titleLabel;
        readonly Label behaviorMetaLabel;
        readonly IMGUIContainer inspectorContainer;

        ComboEditorDocument document;
        AbilityNode selectedNode;
        EditorType cachedNodeEditor;
        EditorType cachedBehaviorEditor;
        InspectorTab currentTab = InspectorTab.Behavior;
        Action<AbilityNode> onNodeChanged;

        public ComboInspectorPane()
        {
            style.flexGrow = 1;
            style.minWidth = 360;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;

            var tabs = new Toolbar();
            var behaviorButton = new ToolbarButton(() => SetTab(InspectorTab.Behavior)) { text = "Behavior" };
            var nodeButton = new ToolbarButton(() => SetTab(InspectorTab.Node)) { text = "Node" };
            tabs.Add(behaviorButton);
            tabs.Add(nodeButton);
            Add(tabs);

            titleLabel = new Label("No node selected");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(titleLabel);

            behaviorMetaLabel = new Label();
            Add(behaviorMetaLabel);

            inspectorContainer = new IMGUIContainer(DrawInspector);
            inspectorContainer.style.flexGrow = 1;
            Add(inspectorContainer);
        }

        public void Bind(ComboEditorDocument document, AbilityNode node, Action<AbilityNode> onNodeChanged)
        {
            this.document = document;
            selectedNode = node;
            this.onNodeChanged = onNodeChanged;
            currentTab = InspectorTab.Behavior;
            RebuildEditors();
            UpdateHeader();
            inspectorContainer.MarkDirtyRepaint();
        }

        public void Refresh()
        {
            UpdateHeader();
            inspectorContainer.MarkDirtyRepaint();
        }

        void SetTab(InspectorTab tab)
        {
            currentTab = tab;
            UpdateHeader();
            inspectorContainer.MarkDirtyRepaint();
        }

        void RebuildEditors()
        {
            if (cachedNodeEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(cachedNodeEditor);
                cachedNodeEditor = null;
            }

            if (cachedBehaviorEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(cachedBehaviorEditor);
                cachedBehaviorEditor = null;
            }

            if (selectedNode != null)
            {
                cachedNodeEditor = EditorType.CreateEditor(selectedNode);
                if (selectedNode.Behavior != null)
                {
                    cachedBehaviorEditor = EditorType.CreateEditor(selectedNode.Behavior);
                }
            }
        }

        void UpdateHeader()
        {
            if (selectedNode == null)
            {
                titleLabel.text = "No node selected";
                behaviorMetaLabel.text = string.Empty;
                return;
            }

            titleLabel.text = $"{selectedNode.name} [{selectedNode.Id}]";
            var behaviorName = selectedNode.Behavior != null ? selectedNode.Behavior.name : "<None>";
            var source = selectedNode.Behavior == null
                ? "-"
                : document != null && document.ContainsLocalBehavior(selectedNode.Behavior) ? "Local" : "External";
            var usageCount = document?.CountBehaviorReferences(selectedNode.Behavior) ?? 0;
            var behaviorPath = selectedNode.Behavior != null ? AssetDatabase.GetAssetPath(selectedNode.Behavior) : string.Empty;
            behaviorMetaLabel.text =
                $"Priority: {selectedNode.Priority} | Conditions: {selectedNode.conditions.Count} | Behavior: {behaviorName} | Source: {source} | Used by: {usageCount}\n{behaviorPath}";
        }

        void DrawInspector()
        {
            if (selectedNode == null)
            {
                EditorGUILayout.HelpBox("Select a combo node to edit its behavior or node settings.", MessageType.Info);
                return;
            }

            if (currentTab == InspectorTab.Behavior)
            {
                DrawBehaviorInspector();
                return;
            }

            DrawNodeInspector();
        }

        void DrawBehaviorInspector()
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Binding", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var assignedBehavior = EditorGUILayout.ObjectField("Behavior Asset", selectedNode.Behavior, typeof(AbilityBehavior), false) as AbilityBehavior;
            if (EditorGUI.EndChangeCheck())
            {
                AssignBehavior(assignedBehavior, false);
                GUIUtility.ExitGUI();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Local Behavior"))
                {
                    ShowCreateBehaviorMenu();
                }

                using (new EditorGUI.DisabledScope(selectedNode.Behavior == null))
                {
                    if (GUILayout.Button("Ping"))
                    {
                        EditorGUIUtility.PingObject(selectedNode.Behavior);
                    }
                }
            }

            if (selectedNode.Behavior == null)
            {
                EditorGUILayout.HelpBox("Assign an existing behavior asset or create a new local behavior for this node.", MessageType.Info);
                return;
            }

            cachedBehaviorEditor ??= EditorType.CreateEditor(selectedNode.Behavior);
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Behavior Inspector", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            cachedBehaviorEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                document?.MarkDirty();
                UpdateHeader();
                onNodeChanged?.Invoke(selectedNode);
            }
        }

        void DrawNodeInspector()
        {
            cachedNodeEditor ??= EditorType.CreateEditor(selectedNode);
            EditorGUI.BeginChangeCheck();
            cachedNodeEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                document?.MarkDirty();
                RebuildEditors();
                UpdateHeader();
                onNodeChanged?.Invoke(selectedNode);
            }
        }

        void ShowCreateBehaviorMenu()
        {
            var behaviorTypes = ComboNodeEditorUtility.GetBehaviorTypes();

            if (behaviorTypes.Count == 0)
            {
                EditorUtility.DisplayDialog("Create Behavior", "No concrete AbilityBehavior types were found.", "OK");
                return;
            }

            var menu = new GenericMenu();
            foreach (var behaviorType in behaviorTypes)
            {
                var capturedType = behaviorType;
                menu.AddItem(new GUIContent(capturedType.Name), false, () => CreateLocalBehavior(capturedType));
            }

            menu.ShowAsContext();
        }

        void CreateLocalBehavior(Type behaviorType)
        {
            if (selectedNode == null || behaviorType == null)
            {
                return;
            }

            var behavior = ComboNodeEditorUtility.CreateLocalBehavior(document, selectedNode, behaviorType);
            if (behavior == null)
            {
                EditorUtility.DisplayDialog("Create Behavior", $"Failed to create behavior of type {behaviorType.Name}.", "OK");
                return;
            }

            RebuildEditors();
            UpdateHeader();
            onNodeChanged?.Invoke(selectedNode);
            EditorGUIUtility.PingObject(behavior);
        }

        void AssignBehavior(AbilityBehavior behavior, bool registerAsLocal)
        {
            if (selectedNode == null)
            {
                return;
            }

            ComboNodeEditorUtility.AssignBehavior(document, selectedNode, behavior, registerAsLocal);
            RebuildEditors();
            UpdateHeader();
            onNodeChanged?.Invoke(selectedNode);
        }
    }
}
