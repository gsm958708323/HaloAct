using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ability.Editor.Combo
{
    public sealed class ComboEditorDocument
    {
        readonly Dictionary<AbilityNode, List<AbilityNode>> edges = new();
        readonly Dictionary<AbilityNode, Rect> positions = new();
        readonly HashSet<AbilityBehavior> localBehaviors = new();

        public ActorComboGraphSO ComboGraph { get; }
        public List<AbilityNode> Nodes { get; } = new();
        public bool IsDirty { get; private set; }

        ComboEditorDocument(ActorComboGraphSO comboGraph, IEnumerable<AbilityNode> nodes)
        {
            ComboGraph = comboGraph;
            Nodes.AddRange(nodes.Where(node => node != null).Distinct());

            if (comboGraph?.LocalBehaviors != null)
            {
                foreach (var behavior in comboGraph.LocalBehaviors.Where(behavior => behavior != null))
                {
                    localBehaviors.Add(behavior);
                }
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                edges[node] = new List<AbilityNode>();
                positions[node] = GetInitialPosition(node, i);
            }
        }

        public static ComboEditorDocument Load(ActorComboGraphSO comboGraph)
        {
            var nodes = comboGraph != null ? comboGraph.Nodes : Enumerable.Empty<AbilityNode>();
            var document = new ComboEditorDocument(comboGraph, nodes);

            if (comboGraph == null)
            {
                return document;
            }

            foreach (var node in document.Nodes)
            {
                foreach (var childId in node.Childs)
                {
                    var target = comboGraph.GetNodeById(childId);
                    if (target != null)
                    {
                        document.edges[node].Add(target);
                    }
                }
            }

            return document;
        }

        public IReadOnlyList<AbilityNode> GetTargets(AbilityNode node)
        {
            return edges.TryGetValue(node, out var targets) ? targets : Array.Empty<AbilityNode>();
        }

        public IReadOnlyList<AbilityBehavior> GetLocalBehaviors()
        {
            return localBehaviors.Where(behavior => behavior != null).OrderBy(behavior => behavior.name).ToList();
        }

        public void Connect(AbilityNode source, AbilityNode target)
        {
            if (source == null || target == null || !edges.TryGetValue(source, out var targets))
            {
                return;
            }

            if (!targets.Contains(target))
            {
                targets.Add(target);
                IsDirty = true;
            }
        }

        public void Disconnect(AbilityNode source, AbilityNode target)
        {
            if (source == null || target == null || !edges.TryGetValue(source, out var targets))
            {
                return;
            }

            if (targets.Remove(target))
            {
                IsDirty = true;
            }
        }

        public void BindBehavior(AbilityNode node, AbilityBehavior behavior)
        {
            if (node == null || node.Behavior == behavior)
            {
                return;
            }

            node.Behavior = behavior;
            IsDirty = true;
        }

        public void RegisterLocalBehavior(AbilityBehavior behavior)
        {
            if (behavior != null && localBehaviors.Add(behavior))
            {
                IsDirty = true;
            }
        }

        public Rect GetPosition(AbilityNode node)
        {
            if (node != null && positions.TryGetValue(node, out var position))
            {
                return position;
            }

            return GetDefaultPosition(node != null ? Nodes.IndexOf(node) : 0);
        }

        public void SetPosition(AbilityNode node, Rect position)
        {
            if (node == null)
            {
                return;
            }

            if (positions.TryGetValue(node, out var currentPosition) && currentPosition == position)
            {
                return;
            }

            positions[node] = position;
            IsDirty = true;
        }

        public bool ContainsLocalBehavior(AbilityBehavior behavior)
        {
            return behavior != null && localBehaviors.Contains(behavior);
        }

        public int CountBehaviorReferences(AbilityBehavior behavior)
        {
            if (behavior == null)
            {
                return 0;
            }

            return Nodes.Count(node => node != null && node.Behavior == behavior);
        }

        public void AddNode(AbilityNode node)
        {
            if (node == null || Nodes.Contains(node))
            {
                return;
            }

            Nodes.Add(node);
            edges[node] = new List<AbilityNode>();
            positions[node] = GetInitialPosition(node, Nodes.Count - 1);
            IsDirty = true;
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        public void MarkClean()
        {
            IsDirty = false;
        }

        static Rect GetDefaultPosition(int index)
        {
            if (index < 0)
            {
                index = 0;
            }

            const float width = 240f;
            const float height = 150f;
            const float gapX = 280f;
            const float gapY = 210f;
            const int columnCount = 4;

            var row = index / columnCount;
            var column = index % columnCount;
            return new Rect(80 + (column * gapX), 120 + (row * gapY), width, height);
        }

        static Rect GetInitialPosition(AbilityNode node, int index)
        {
            var defaultPosition = GetDefaultPosition(index);
            if (node == null || node.EditorPosition == Vector2.zero)
            {
                return defaultPosition;
            }

            return new Rect(node.EditorPosition, defaultPosition.size);
        }
    }
}
