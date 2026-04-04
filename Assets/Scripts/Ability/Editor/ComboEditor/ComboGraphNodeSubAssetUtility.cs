using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ability.Editor.Combo
{
    public static class ComboGraphNodeSubAssetUtility
    {
        [MenuItem("Tools/Ability/Embed Combo Graph Nodes")]
        static void EmbedAllGraphsMenu()
        {
            var changedCount = EmbedAllGraphs();
            EditorUtility.DisplayDialog("Embed Combo Graph Nodes", $"Updated {changedCount} combo graph assets.", "OK");
        }

        public static void EmbedAllGraphsBatchmode()
        {
            EmbedAllGraphs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static int EmbedAllGraphs()
        {
            var changedCount = 0;
            var graphGuids = AssetDatabase.FindAssets("t:ActorComboGraphSO");
            Debug.Log($"[ComboGraphNodeSubAssetUtility] Found {graphGuids.Length} combo graphs.");
            for (int i = 0; i < graphGuids.Length; i++)
            {
                var graphPath = AssetDatabase.GUIDToAssetPath(graphGuids[i]);
                var comboGraph = AssetDatabase.LoadAssetAtPath<ActorComboGraphSO>(graphPath);
                Debug.Log($"[ComboGraphNodeSubAssetUtility] Processing graph: {graphPath}");
                if (EnsureNodesAreEmbedded(comboGraph))
                {
                    changedCount++;
                    Debug.Log($"[ComboGraphNodeSubAssetUtility] Updated graph: {graphPath}");
                }
            }

            return changedCount;
        }

        public static bool EnsureNodesAreEmbedded(ActorComboGraphSO comboGraph)
        {
            if (comboGraph == null)
            {
                return false;
            }

            var graphPath = AssetDatabase.GetAssetPath(comboGraph);
            if (string.IsNullOrEmpty(graphPath))
            {
                return false;
            }

            var nodes = comboGraph.Nodes ?? new List<AbilityNode>();
            var normalizedNodes = new List<AbilityNode>(nodes.Count);
            var embeddedLookup = new Dictionary<AbilityNode, AbilityNode>();
            var changed = false;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null)
                {
                    changed = true;
                    continue;
                }

                if (embeddedLookup.TryGetValue(node, out var cachedNode))
                {
                    normalizedNodes.Add(cachedNode);
                    continue;
                }

                var embeddedNode = GetEmbeddedNode(comboGraph, node, graphPath);
                if (embeddedNode != node)
                {
                    changed = true;
                    Debug.Log($"[ComboGraphNodeSubAssetUtility] Embedded node '{node.name}' into '{comboGraph.name}'.");
                }

                embeddedLookup[node] = embeddedNode;
                normalizedNodes.Add(embeddedNode);
            }

            var orderedNodes = normalizedNodes
                .Where(node => node != null)
                .Distinct()
                .OrderBy(node => node.Id)
                .ToList();

            if (!changed && orderedNodes.SequenceEqual(comboGraph.Nodes))
            {
                return false;
            }

            comboGraph.Nodes = orderedNodes;
            EditorUtility.SetDirty(comboGraph);
            Debug.Log($"[ComboGraphNodeSubAssetUtility] Saving graph '{comboGraph.name}' with {orderedNodes.Count} embedded nodes.");
            AssetDatabase.ImportAsset(graphPath);
            AssetDatabase.SaveAssets();
            return true;
        }

        static AbilityNode GetEmbeddedNode(ActorComboGraphSO comboGraph, AbilityNode node, string graphPath)
        {
            var nodePath = AssetDatabase.GetAssetPath(node);
            if (nodePath == graphPath)
            {
                return node;
            }

            var embeddedNode = Object.Instantiate(node);
            embeddedNode.name = node.name;
            AssetDatabase.AddObjectToAsset(embeddedNode, comboGraph);
            EditorUtility.SetDirty(embeddedNode);
            return embeddedNode;
        }
    }
}
