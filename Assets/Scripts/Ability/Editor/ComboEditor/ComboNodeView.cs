using System;
using System.Linq;
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
        readonly Action<AbilityNode, Rect> onMoved;
        bool suppressPositionNotification;

        public AbilityNode NodeAsset { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        public ComboNodeView(AbilityNode nodeAsset, Action<AbilityNode, Rect> onMoved)
        {
            NodeAsset = nodeAsset;
            this.onMoved = onMoved;
            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Ascendable | Capabilities.Deletable;
            pickingMode = PickingMode.Position;

            title = $"{nodeAsset.name} [{nodeAsset.Id}]";
            viewDataKey = $"combo-node-{nodeAsset.Id}";
            style.minWidth = 240;
            style.minHeight = 140;

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
            summaryContainer.style.minHeight = 72;
            mainContainer.Add(summaryContainer);

            priorityLabel = new Label();
            conditionsLabel = new Label();
            behaviorLabel = new Label();

            summaryContainer.Add(priorityLabel);
            summaryContainer.Add(conditionsLabel);
            summaryContainer.Add(behaviorLabel);

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
            title = $"{NodeAsset.name} [{NodeAsset.Id}]";
            priorityLabel.text = $"Priority: {NodeAsset.Priority}";
            // conditionsLabel.text = $"Conditions: {this.BuildConditionSummary()}";
            behaviorLabel.text = $"Behavior: {(NodeAsset.Behavior != null ? NodeAsset.Behavior.name : "<None>")}";
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            if (!suppressPositionNotification)
            {
                onMoved?.Invoke(NodeAsset, newPos);
            }
        }

        string BuildConditionSummary(AbilityNode node)
        {
            if (node == null || node.conditions == null || node.conditions.Count == 0)
            {
                return "None";
            }

            return string.Join(", ", node.conditions
                .Where(condition => condition != null)
                .Select(condition => condition.ToString())
                .DefaultIfEmpty("<Missing>"));
        }
    }
}
