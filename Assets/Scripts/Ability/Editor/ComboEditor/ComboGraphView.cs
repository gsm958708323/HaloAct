using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ability.Editor.Combo
{
    public class ComboGraphView : GraphView
    {
        readonly Dictionary<AbilityNode, ComboNodeView> nodeViews = new();
        ComboEditorDocument document;
        Action<AbilityNode> onNodeSelected;
        Action<AbilityNode> onNodeChanged;
        Action<Vector2> onCreateNodeRequested;
        bool isRebuilding;
        bool suppressSelectionSync;

        public ComboGraphView()
        {
            style.flexGrow = 1;

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            graphViewChanged = OnGraphViewChanged;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (port == startPort || port.node == startPort.node)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            SyncSelection();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            SyncSelection();
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            SyncSelection();
        }

        public void Bind(ComboEditorDocument document, Action<AbilityNode> onNodeSelected, Action<AbilityNode> onNodeChanged, Action<Vector2> onCreateNodeRequested)
        {
            this.document = document;
            this.onNodeSelected = onNodeSelected;
            this.onNodeChanged = onNodeChanged;
            this.onCreateNodeRequested = onCreateNodeRequested;
            Rebuild();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (document == null)
            {
                return;
            }

            var localPosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("Create Node", _ => onCreateNodeRequested?.Invoke(localPosition), DropdownMenuAction.AlwaysEnabled);
        }

        public void Rebuild()
        {
            var selectedNode = GetSelectedNode();

            isRebuilding = true;
            suppressSelectionSync = true;
            base.ClearSelection();
            DeleteElements(graphElements.ToList());
            nodeViews.Clear();

            if (document == null)
            {
                isRebuilding = false;
                suppressSelectionSync = false;
                SyncSelection();
                return;
            }

            for (int i = 0; i < document.Nodes.Count; i++)
            {
                var node = document.Nodes[i];
                if (node == null)
                {
                    continue;
                }

                var nodeView = new ComboNodeView(document, node, HandleNodeChanged, HandleNodeMoved);
                nodeView.ApplyPosition(document.GetPosition(node));
                nodeViews[node] = nodeView;
                AddElement(nodeView);
            }

            foreach (var node in document.Nodes)
            {
                if (node == null || !nodeViews.TryGetValue(node, out var sourceView))
                {
                    continue;
                }

                foreach (var target in document.GetTargets(node))
                {
                    if (!nodeViews.TryGetValue(target, out var targetView))
                    {
                        continue;
                    }

                    var edge = sourceView.OutputPort.ConnectTo(targetView.InputPort);
                    AddElement(edge);
                }
            }

            isRebuilding = false;
            suppressSelectionSync = false;

            if (selectedNode != null && nodeViews.TryGetValue(selectedNode, out var selectedNodeView))
            {
                base.AddToSelection(selectedNodeView);
            }

            SyncSelection();
        }

        public void RefreshNode(AbilityNode node)
        {
            if (node != null && nodeViews.TryGetValue(node, out var nodeView))
            {
                nodeView.RefreshSummary();
            }
        }

        public void SelectNode(AbilityNode node)
        {
            if (node == null || !nodeViews.TryGetValue(node, out var nodeView))
            {
                return;
            }

            suppressSelectionSync = true;
            base.ClearSelection();
            base.AddToSelection(nodeView);
            suppressSelectionSync = false;
            SyncSelection();
        }

        public void AutoLayout()
        {
            if (document == null)
            {
                return;
            }

            var nodes = document.Nodes.Where(node => node != null).OrderBy(node => node.Id).ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                var position = ComboGraphLayout.GetDefaultPosition(i);
                document.SetPosition(nodes[i], position);
            }

            Rebuild();
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (document == null || isRebuilding)
            {
                return change;
            }

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    var source = (edge.output.node as ComboNodeView)?.NodeAsset;
                    var target = (edge.input.node as ComboNodeView)?.NodeAsset;
                    document.Connect(source, target);
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        var source = (edge.output.node as ComboNodeView)?.NodeAsset;
                        var target = (edge.input.node as ComboNodeView)?.NodeAsset;
                        document.Disconnect(source, target);
                    }
                    else if (element is ComboNodeView nodeView)
                    {
                        document.RemoveNode(nodeView.NodeAsset);
                    }
                }
            }

            return change;
        }

        AbilityNode GetSelectedNode()
        {
            for (int i = selection.Count - 1; i >= 0; i--)
            {
                if (selection[i] is ComboNodeView nodeView)
                {
                    return nodeView.NodeAsset;
                }
            }

            return null;
        }

        void SyncSelection()
        {
            if (isRebuilding || suppressSelectionSync)
            {
                return;
            }

            onNodeSelected?.Invoke(GetSelectedNode());
        }

        void HandleNodeMoved(AbilityNode node, Rect position)
        {
            document?.SetPosition(node, position);
        }

        void HandleNodeChanged(AbilityNode node)
        {
            if (node == null)
            {
                return;
            }

            RefreshNode(node);
            onNodeChanged?.Invoke(node);
        }
    }
}
