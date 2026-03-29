using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ability
{
    public static class ComboGraphBindingUtility
    {
        static readonly Regex DigitRegex = new Regex(@"\d", RegexOptions.Compiled);

        public static void ApplyLegacyBindings(IEnumerable<AbilityNode> nodes, IEnumerable<AbilityBehavior> behaviors)
        {
            if (nodes == null)
            {
                return;
            }

            var behaviorMap = BuildBehaviorMap(behaviors);
            foreach (var node in nodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (TryResolveBehavior(node, behaviorMap, out var behavior))
                {
                    node.Behavior = behavior;
                }
            }
        }

        public static bool TryResolveBehavior(AbilityNode node, IEnumerable<AbilityBehavior> behaviors, out AbilityBehavior behavior)
        {
            return TryResolveBehavior(node, BuildBehaviorMap(behaviors), out behavior);
        }

        static Dictionary<string, AbilityBehavior> BuildBehaviorMap(IEnumerable<AbilityBehavior> behaviors)
        {
            var map = new Dictionary<string, AbilityBehavior>();
            if (behaviors == null)
            {
                return map;
            }

            foreach (var behavior in behaviors.Where(item => item != null))
            {
                map[behavior.name] = behavior;
            }

            return map;
        }

        static bool TryResolveBehavior(AbilityNode node, IReadOnlyDictionary<string, AbilityBehavior> behaviorMap, out AbilityBehavior behavior)
        {
            behavior = null;
            if (node == null || behaviorMap == null || behaviorMap.Count == 0)
            {
                return false;
            }

            if (behaviorMap.TryGetValue(node.name, out behavior))
            {
                return true;
            }

            var strippedName = DigitRegex.Replace(node.name, string.Empty);
            return behaviorMap.TryGetValue(strippedName, out behavior);
        }
    }
}
