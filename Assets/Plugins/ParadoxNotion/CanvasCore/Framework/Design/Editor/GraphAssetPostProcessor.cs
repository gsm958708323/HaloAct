#if UNITY_EDITOR

using UnityEditor;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Editor
{
    ///<summary>Handles post processing of graph assets</summary>
    public class GraphAssetPostProcessor
    {

        [InitializeOnLoadMethod]
        static void PreInit() {
            EditorApplication.delayCall -= Init;
            EditorApplication.delayCall += Init;
        }

        static void Init() {
            //we track graph assets so that we can access them on a diff thread
            AssetTracker.BeginTrackingAssetsOfType(typeof(Graph));
        }
    }
}

#endif