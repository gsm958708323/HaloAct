using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ability.Editor.Combo
{
    public class ComboNodeView : Node
    {
        readonly ComboEditorDocument document;
        readonly Action<AbilityNode> onChanged;
        readonly Action<AbilityNode, Rect> onMoved;
        readonly IntegerField priorityField;
        readonly Foldout conditionsFoldout;
        readonly VisualElement conditionsContainer;
        readonly ObjectField behaviorField;
        readonly Button createBehaviorButton;
        readonly Button pingBehaviorButton;
        bool suppressPositionNotification;
        bool suppressValueCallbacks;

        public AbilityNode NodeAsset { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        public ComboNodeView(ComboEditorDocument document, AbilityNode nodeAsset, Action<AbilityNode> onChanged, Action<AbilityNode, Rect> onMoved)
        {
            this.document = document;
            NodeAsset = nodeAsset;
            this.onChanged = onChanged;
            this.onMoved = onMoved;
            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Ascendable | Capabilities.Deletable;
            pickingMode = PickingMode.Position;

            title = $"{nodeAsset.name} [{nodeAsset.Id}]";
            viewDataKey = $"combo-node-{nodeAsset.Id}";
            style.minWidth = 280;
            style.minHeight = 210;

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            var summaryContainer = new VisualElement();
            summaryContainer.style.paddingLeft = 6;
            summaryContainer.style.paddingRight = 6;
            summaryContainer.style.paddingTop = 4;
            summaryContainer.style.paddingBottom = 6;
            summaryContainer.style.minHeight = 130;
            mainContainer.Add(summaryContainer);

            priorityField = new IntegerField("Priority");
            priorityField.RegisterValueChangedCallback(OnPriorityChanged);
            summaryContainer.Add(priorityField);

            conditionsFoldout = new Foldout();
            conditionsFoldout.value = false;
            conditionsContainer = new VisualElement();
            conditionsContainer.style.marginLeft = 4;
            conditionsFoldout.Add(conditionsContainer);
            summaryContainer.Add(conditionsFoldout);

            behaviorField = new ObjectField("Behavior")
            {
                objectType = typeof(AbilityBehavior),
                allowSceneObjects = false
            };
            behaviorField.RegisterValueChangedCallback(OnBehaviorChanged);
            summaryContainer.Add(behaviorField);

            var behaviorButtons = new VisualElement();
            behaviorButtons.style.flexDirection = FlexDirection.Row;
            behaviorButtons.style.justifyContent = Justify.FlexEnd;
            // behaviorButtons.style.gap = 4;

            createBehaviorButton = new Button(ShowCreateBehaviorMenu) { text = "New" };
            pingBehaviorButton = new Button(PingBehavior) { text = "Ping" };

            behaviorButtons.Add(createBehaviorButton);
            behaviorButtons.Add(pingBehaviorButton);
            summaryContainer.Add(behaviorButtons);

            RefreshSummary();
            RefreshExpandedState();
            RefreshPorts();
        }

        public void ApplyPosition(Rect position)
        {
            suppressPositionNotification = true;
            SetPosition(position);
            suppressPositionNotification = false;
        }

        public void RefreshSummary()
        {
            suppressValueCallbacks = true;
            title = $"{NodeAsset.name} [{NodeAsset.Id}]";
            priorityField.SetValueWithoutNotify(NodeAsset.Priority);
            behaviorField.SetValueWithoutNotify(NodeAsset.Behavior);
            pingBehaviorButton.SetEnabled(NodeAsset.Behavior != null);
            RebuildConditions();
            suppressValueCallbacks = false;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            if (!suppressPositionNotification)
            {
                onMoved?.Invoke(NodeAsset, newPos);
            }
        }

        void OnPriorityChanged(ChangeEvent<int> evt)
        {
            if (suppressValueCallbacks)
            {
                return;
            }

            ComboNodeEditorUtility.SetPriority(document, NodeAsset, evt.newValue);
            NotifyNodeChanged();
        }

        void OnBehaviorChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            if (suppressValueCallbacks)
            {
                return;
            }

            ComboNodeEditorUtility.AssignBehavior(document, NodeAsset, evt.newValue as AbilityBehavior);
            RefreshSummary();
            NotifyNodeChanged();
        }

        void RebuildConditions()
        {
            conditionsContainer.Clear();

            var conditionCount = NodeAsset.conditions?.Count ?? 0;
            conditionsFoldout.text = $"Conditions ({conditionCount})";

            if (conditionCount == 0)
            {
                var emptyLabel = new Label("None");
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                conditionsContainer.Add(emptyLabel);
            }
            else
            {
                for (int i = 0; i < NodeAsset.conditions.Count; i++)
                {
                    var condition = NodeAsset.conditions[i];
                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.alignItems = Align.Center;
                    row.style.justifyContent = Justify.SpaceBetween;

                    var label = new Label(condition != null ? condition.GetType().Name : "<Missing>");
                    label.style.flexGrow = 1;

                    var removeIndex = i;
                    var removeButton = new Button(() => RemoveCondition(removeIndex)) { text = "X" };
                    removeButton.style.width = 24;

                    row.Add(label);
                    row.Add(removeButton);
                    conditionsContainer.Add(row);
                }
            }

            var addConditionButton = new Button(ShowAddConditionMenu) { text = "Add Condition" };
            conditionsContainer.Add(addConditionButton);
        }

        void ShowAddConditionMenu()
        {
            var conditionTypes = ComboNodeEditorUtility.GetConditionTypes();
            if (conditionTypes.Count == 0)
            {
                EditorUtility.DisplayDialog("Add Condition", "No concrete AbilityCondition types were found.", "OK");
                return;
            }

            var menu = new GenericMenu();
            foreach (var conditionType in conditionTypes)
            {
                var capturedType = conditionType;
                menu.AddItem(new GUIContent(capturedType.Name), false, () => AddCondition(capturedType));
            }

            menu.ShowAsContext();
        }

        void AddCondition(Type conditionType)
        {
            ComboNodeEditorUtility.AddCondition(document, NodeAsset, conditionType);
            RefreshSummary();
            NotifyNodeChanged();
        }

        void RemoveCondition(int index)
        {
            ComboNodeEditorUtility.RemoveConditionAt(document, NodeAsset, index);
            RefreshSummary();
            NotifyNodeChanged();
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
                menu.AddItem(new GUIContent(capturedType.Name), false, () => CreateBehavior(capturedType));
            }

            menu.ShowAsContext();
        }

        void CreateBehavior(Type behaviorType)
        {
            var behavior = ComboNodeEditorUtility.CreateLocalBehavior(document, NodeAsset, behaviorType);
            if (behavior == null)
            {
                EditorUtility.DisplayDialog("Create Behavior", $"Failed to create behavior of type {behaviorType.Name}.", "OK");
                return;
            }

            RefreshSummary();
            NotifyNodeChanged();
            EditorGUIUtility.PingObject(behavior);
        }

        void PingBehavior()
        {
            if (NodeAsset.Behavior != null)
            {
                EditorGUIUtility.PingObject(NodeAsset.Behavior);
            }
        }

        void NotifyNodeChanged()
        {
            onChanged?.Invoke(NodeAsset);
        }
    }
}
