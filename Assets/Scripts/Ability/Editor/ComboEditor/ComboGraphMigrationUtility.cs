using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ability.Editor.Combo
{
    public static class ComboGraphMigrationUtility
    {
        [MenuItem("Assets/Ability/Migrate Combo Graph", true)]
        static bool ValidateMigrateSelectedActor()
        {
            return Selection.activeObject is ActorData;
        }

        [MenuItem("Assets/Ability/Migrate Combo Graph")]
        static void MigrateSelectedActor()
        {
            if (Selection.activeObject is not ActorData actor)
            {
                return;
            }

            if (!Migrate(actor, out _, out var message))
            {
                EditorUtility.DisplayDialog("Combo Migration Failed", message, "OK");
                return;
            }

            EditorUtility.DisplayDialog("Combo Migration", message, "OK");
        }

        public static bool Migrate(ActorData actor, out ActorComboGraphSO comboGraph, out string message)
        {
            comboGraph = null;

            if (actor == null)
            {
                message = "Actor is null.";
                return false;
            }

            var actorAssetPath = AssetDatabase.GetAssetPath(actor);
            if (string.IsNullOrEmpty(actorAssetPath))
            {
                message = "Actor must be saved as an asset before migration.";
                return false;
            }

            var nodeFolder = ResolveResourcesFolderAssetPath(actor.NodePath);
            var behaviorFolder = ResolveResourcesFolderAssetPath(actor.BehaviorPath);
            if (string.IsNullOrEmpty(nodeFolder))
            {
                message = $"Cannot resolve node folder from Resources path '{actor.NodePath}'.";
                return false;
            }

            if (string.IsNullOrEmpty(behaviorFolder))
            {
                message = $"Cannot resolve behavior folder from Resources path '{actor.BehaviorPath}'.";
                return false;
            }

            var nodes = LoadAssetsInFolder<AbilityNode>(nodeFolder).OrderBy(node => node.Id).ToList();
            var behaviors = LoadAssetsInFolder<AbilityBehavior>(behaviorFolder).OrderBy(behavior => behavior.name).ToList();
            if (nodes.Count == 0)
            {
                message = $"No nodes found in '{nodeFolder}'.";
                return false;
            }

            comboGraph = GetOrCreateComboGraph(actor, actorAssetPath);
            ComboGraphBindingUtility.ApplyLegacyBindings(nodes, behaviors);

            comboGraph.name = $"{actor.name}_ComboGraph";
            comboGraph.Nodes = nodes;
            comboGraph.LocalBehaviors = behaviors;
            actor.ComboGraph = comboGraph;

            EditorUtility.SetDirty(comboGraph);
            EditorUtility.SetDirty(actor);

            foreach (var node in nodes)
            {
                EditorUtility.SetDirty(node);
            }

            foreach (var behavior in behaviors)
            {
                EditorUtility.SetDirty(behavior);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            message = $"Migrated {actor.name}: {nodes.Count} nodes, {behaviors.Count} behaviors.";
            return true;
        }

        static ActorComboGraphSO GetOrCreateComboGraph(ActorData actor, string actorAssetPath)
        {
            if (actor.ComboGraph != null)
            {
                return actor.ComboGraph;
            }

            var directory = Path.GetDirectoryName(actorAssetPath)?.Replace("\\", "/");
            var comboPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{actor.name}_ComboGraph.asset");
            var comboGraph = ScriptableObject.CreateInstance<ActorComboGraphSO>();
            AssetDatabase.CreateAsset(comboGraph, comboPath);
            return comboGraph;
        }

        static List<T> LoadAssetsInFolder<T>(string assetFolder) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetFolder) || !AssetDatabase.IsValidFolder(assetFolder))
            {
                return new List<T>();
            }

            var assetPaths = Directory.GetFiles(ToFullPath(assetFolder), "*.asset", SearchOption.TopDirectoryOnly)
                .Select(ToAssetPath)
                .OrderBy(path => path);

            var assets = new List<T>();
            foreach (var assetPath in assetPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        static string ResolveResourcesFolderAssetPath(string resourcesPath)
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
            {
                return null;
            }

            var relativeResourcesPath = resourcesPath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            foreach (var resourcesFolder in Directory.GetDirectories(Application.dataPath, "Resources", SearchOption.AllDirectories))
            {
                var candidate = Path.Combine(resourcesFolder, relativeResourcesPath);
                if (Directory.Exists(candidate))
                {
                    return ToAssetPath(candidate);
                }
            }

            return null;
        }

        static string ToAssetPath(string fullPath)
        {
            var normalized = fullPath.Replace("\\", "/");
            var assetsPath = Application.dataPath.Replace("\\", "/");
            return normalized.StartsWith(assetsPath)
                ? $"Assets{normalized.Substring(assetsPath.Length)}"
                : normalized;
        }

        static string ToFullPath(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            return Path.Combine(projectRoot, assetPath).Replace("/", "\\");
        }
    }
}
