using System.Collections.Generic;
using System.Linq;

namespace Ability
{
    public sealed class ComboGraphValidationResult
    {
        public readonly List<string> Errors = new();

        public bool IsValid => Errors.Count == 0;
    }

    public static class ComboGraphValidation
    {
        public static ComboGraphValidationResult Validate(ActorComboGraphSO comboGraph)
        {
            return comboGraph == null
                ? Validate((IEnumerable<AbilityNode>)null)
                : Validate(comboGraph.Nodes);
        }

        public static ComboGraphValidationResult Validate(IEnumerable<AbilityNode> nodes)
        {
            var result = new ComboGraphValidationResult();
            if (nodes == null)
            {
                result.Errors.Add("combo graph is missing");
                return result;
            }

            var nodeList = nodes.Where(node => node != null).ToList();
            if (nodeList.Count == 0)
            {
                result.Errors.Add("combo graph has no nodes");
                return result;
            }

            if (nodeList.All(node => node.Id != 0))
            {
                result.Errors.Add("combo graph is missing root node id 0");
            }

            var ids = new HashSet<int>();
            foreach (var node in nodeList)
            {
                if (!ids.Add(node.Id))
                {
                    result.Errors.Add($"duplicate node id: {node.Id}");
                }

                if (node.Behavior == null)
                {
                    result.Errors.Add($"node {node.Id} is missing behavior");
                }
            }

            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                for (int j = 0; j < node.Childs.Count; j++)
                {
                    var childId = node.Childs[j];
                    if (!ids.Contains(childId))
                    {
                        result.Errors.Add($"node {node.Id} references missing child id {childId}");
                    }
                }
            }

            return result;
        }
    }
}
