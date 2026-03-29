using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ability
{
    [CreateAssetMenu(fileName = "NewComboGraph", menuName = "AbilityTree/ComboGraph")]
    public class ActorComboGraphSO : SerializedScriptableObject
    {
        public List<AbilityNode> Nodes = new();
        public List<AbilityBehavior> LocalBehaviors = new();

        public AbilityNode GetRootNode()
        {
            return GetNodeById(0);
        }

        public AbilityNode GetNodeById(int id)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                if (node != null && node.Id == id)
                {
                    return node;
                }
            }

            return null;
        }

        public IReadOnlyDictionary<int, AbilityNode> BuildNodeMap()
        {
            var nodeMap = new Dictionary<int, AbilityNode>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                if (node == null)
                {
                    continue;
                }

                nodeMap[node.Id] = node;
            }

            return nodeMap;
        }
    }
}
