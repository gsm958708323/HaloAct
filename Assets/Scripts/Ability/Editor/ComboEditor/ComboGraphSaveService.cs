using System.Linq;
using UnityEditor;

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

                node.EditorPosition = document.GetPosition(node).position;
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

            document.ComboGraph.LocalBehaviors = document.Nodes
                .Select(node => node.Behavior)
                .Where(behavior => behavior != null)
                .Distinct()
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

            AssetDatabase.SaveAssets();
            document.MarkClean();
            return true;
        }
    }
}
