using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ability.Editor.Combo
{
    public static class ComboGraphSaveService
    {
        public static void Apply(ComboEditorDocument document)
        {
            if (document == null || document.ComboGraph == null)
            {
                return;
            }

            foreach (var node in document.Nodes)
            {
                if (node == null)
                {
                    continue;
                }

                var nodeRect = document.GetPosition(node);
                node.EditorPosition = nodeRect.position;
                node.EditorRect = nodeRect;
                node.HasEditorPosition = true;
                node.Childs = document.GetTargets(node)
                    .Where(target => target != null)
                    .Select(target => target.Id)
                    .Distinct()
                    .OrderBy(id => id)
                    .ToList();
            }

            document.ComboGraph.Nodes = document.Nodes
                .Where(node => node != null)
                .OrderBy(node => node.Id)
                .ToList();

            document.ComboGraph.LocalBehaviors = document.GetLocalBehaviors()
                .Where(behavior => behavior != null)
                .ToList();
        }

        public static bool Save(ComboEditorDocument document, out ComboGraphValidationResult validation)
        {
            Apply(document);
            validation = ComboGraphValidation.Validate(document.ComboGraph);
            if (!validation.IsValid)
            {
                return false;
            }

            EditorUtility.SetDirty(document.ComboGraph);

            foreach (var node in document.Nodes)
            {
                if (node != null)
                {
                    EditorUtility.SetDirty(node);
                }

                if (node?.Behavior != null)
                {
                    EditorUtility.SetDirty(node.Behavior);
                }
            }

            foreach (var behavior in document.GetLocalBehaviors())
            {
                if (behavior != null)
                {
                    EditorUtility.SetDirty(behavior);
                }
            }

            foreach (var removedNode in document.GetRemovedNodes())
            {
                if (removedNode == null)
                {
                    continue;
                }

                Object.DestroyImmediate(removedNode, true);
            }

            AssetDatabase.SaveAssets();
            document.ClearRemovedNodes();
            document.MarkClean();
            return true;
        }
    }
}
