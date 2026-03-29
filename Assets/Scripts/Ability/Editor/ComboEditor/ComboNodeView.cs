using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ability.Editor.Combo
{
    public class ComboNodeView : Node
    {
        readonly Label priorityLabel;
        readonly Label conditionsLabel;
        readonly Label behaviorLabel;
        readonly Action<AbilityNode> onSelected;
        readonly Action<AbilityNode, Rect> onMoved;
        bool suppressPositionNotification;

        public AbilityNode NodeAsset { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        public ComboNodeView(AbilityNode nodeAsset, Action<AbilityNode> onSelected, Action<AbilityNode, Rect> onMoved)
        {
            NodeAsset = nodeAsset;
            this.onSelected = onSelected;
            this.onMoved = onMoved;

            capabilities |= Capabilities.Selectable | Capabilities.Movable;
            pickingMode = PickingMode.Position;

            title = $"{nodeAsset.name} [{nodeAsset.Id}]";
            viewDataKey = $"combo-node-{nodeAsset.Id}";

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            priorityLabel = new Label();
            conditionsLabel = new Label();
            behaviorLabel = new Label();
            extensionContainer.Add(priorityLabel);
            extensionContainer.Add(conditionsLabel);
            extensionContainer.Add(behaviorLabel);

            RefreshSummary();
            RefreshExpandedState();
            RefreshPorts();
            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    onSelected?.Invoke(NodeAsset);
                }
            });
        }

        public void ApplyPosition(Rect position)
        {
            suppressPositionNotification = true;
            SetPosition(position);
            suppressPositionNotification = false;
        }

        public void RefreshSummary()
        {
            title = $"{NodeAsset.name} [{NodeAsset.Id}]";
            priorityLabel.text = $"Priority: {NodeAsset.Priority}";
            conditionsLabel.text = $"Conditions: {NodeAsset.conditions.Count}";
            behaviorLabel.text = $"Behavior: {(NodeAsset.Behavior != null ? NodeAsset.Behavior.name : "<None>")}";
        }

        public override void OnSelected()
        {
            base.OnSelected();
            onSelected?.Invoke(NodeAsset);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            if (!suppressPositionNotification)
            {
                onMoved?.Invoke(NodeAsset, newPos);
            }
        }
    }
}
