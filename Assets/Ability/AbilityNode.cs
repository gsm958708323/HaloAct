using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ability
{
    [CreateAssetMenu(fileName = "NewNode", menuName = "AbilityTree/AbilityNode")]
    /// <summary>
    /// 记录行为节点的连招过渡关系和其他拓展信息
    /// </summary>
    public class AbilityNode : SerializedScriptableObject
    {
        /// <summary>
        /// 所有的叶子节点的索引
        /// </summary>
        /// <returns></returns>
        public List<int> Childs = new();

        public int Priority;
        public int BehaviorIndex;
    }
}