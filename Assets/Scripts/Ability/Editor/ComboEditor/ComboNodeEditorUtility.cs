using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ability.Editor.Combo
{
    static class ComboNodeEditorUtility
    {
        static IReadOnlyList<Type> cachedBehaviorTypes;
        static IReadOnlyList<Type> cachedConditionTypes;

        public static IReadOnlyList<Type> GetBehaviorTypes()
        {
            cachedBehaviorTypes ??= TypeCache.GetTypesDerivedFrom<AbilityBehavior>()
                .Where(type => type != null && !type.IsAbstract && !type.IsGenericType)
                .OrderBy(type => type.Name)
                .ToList();
            return cachedBehaviorTypes;
        }

        public static IReadOnlyList<Type> GetConditionTypes()
        {
            cachedConditionTypes ??= TypeCache.GetTypesDerivedFrom<AbilityCondition>()
                .Where(type => type != null && !type.IsAbstract && !type.IsGenericType)
                .OrderBy(type => type.Name)
                .ToList();
            return cachedConditionTypes;
        }

        public static void SetPriority(ComboEditorDocument document, AbilityNode node, int priority)
        {
            if (node == null || node.Priority == priority)
            {
                return;
            }

            node.Priority = priority;
            MarkNodeDirty(document, node);
        }

        public static void AssignBehavior(ComboEditorDocument document, AbilityNode node, AbilityBehavior behavior, bool registerAsLocal = false)
        {
            if (document == null || node == null || node.Behavior == behavior)
            {
                return;
            }

            document.BindBehavior(node, behavior);
            if (registerAsLocal || IsBehaviorStoredNearGraph(document, behavior))
            {
                document.RegisterLocalBehavior(behavior);
            }

            EditorUtility.SetDirty(node);
            if (behavior != null)
            {
                EditorUtility.SetDirty(behavior);
            }
        }

        public static AbilityBehavior CreateLocalBehavior(ComboEditorDocument document, AbilityNode node, Type behaviorType)
        {
            if (document?.ComboGraph == null || node == null || behaviorType == null)
            {
                return null;
            }

            var directory = GetBehaviorDirectory(document, node);
            if (string.IsNullOrEmpty(directory))
            {
                return null;
            }

            var suffix = behaviorType.Name.StartsWith("AbilityBehavior", StringComparison.Ordinal)
                ? behaviorType.Name.Substring("AbilityBehavior".Length)
                : behaviorType.Name;
            if (string.IsNullOrEmpty(suffix))
            {
                suffix = "Behavior";
            }

            var behavior = ScriptableObject.CreateInstance(behaviorType) as AbilityBehavior;
            if (behavior == null)
            {
                return null;
            }

            behavior.name = $"{node.name}_{suffix}";
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, $"{behavior.name}.asset").Replace("\\", "/"));
            AssetDatabase.CreateAsset(behavior, assetPath);
            AssetDatabase.SaveAssets();

            AssignBehavior(document, node, behavior, true);
            return behavior;
        }

        public static void AddCondition(ComboEditorDocument document, AbilityNode node, Type conditionType)
        {
            if (node == null || conditionType == null)
            {
                return;
            }

            if (Activator.CreateInstance(conditionType) is not AbilityCondition condition)
            {
                return;
            }

            node.conditions ??= new List<AbilityCondition>();
            node.conditions.Add(condition);
            MarkNodeDirty(document, node);
        }

        public static void RemoveConditionAt(ComboEditorDocument document, AbilityNode node, int index)
        {
            if (node?.conditions == null || index < 0 || index >= node.conditions.Count)
            {
                return;
            }

            node.conditions.RemoveAt(index);
            MarkNodeDirty(document, node);
        }

        public static string GetBehaviorDirectory(ComboEditorDocument document, AbilityNode node)
        {
            if (node != null)
            {
                var nodePath = AssetDatabase.GetAssetPath(node);
                var nodeDirectory = Path.GetDirectoryName(nodePath);
                if (!string.IsNullOrEmpty(nodeDirectory))
                {
                    return nodeDirectory.Replace("\\", "/");
                }
            }

            var graphPath = document?.ComboGraph != null ? AssetDatabase.GetAssetPath(document.ComboGraph) : string.Empty;
            var graphDirectory = Path.GetDirectoryName(graphPath);
            return graphDirectory?.Replace("\\", "/");
        }

        static bool IsBehaviorStoredNearGraph(ComboEditorDocument document, AbilityBehavior behavior)
        {
            if (document?.ComboGraph == null || behavior == null)
            {
                return false;
            }

            var graphDirectory = GetBehaviorDirectory(document, null);
            var behaviorPath = AssetDatabase.GetAssetPath(behavior)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(graphDirectory) || string.IsNullOrEmpty(behaviorPath))
            {
                return false;
            }

            return behaviorPath.StartsWith(graphDirectory, StringComparison.OrdinalIgnoreCase);
        }

        static void MarkNodeDirty(ComboEditorDocument document, AbilityNode node)
        {
            if (node == null)
            {
                return;
            }

            document?.MarkDirty();
            EditorUtility.SetDirty(node);
        }
    }
}
