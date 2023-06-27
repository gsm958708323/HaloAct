using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 记录行为节点的连招过渡关系和其他拓展信息
    /// </summary>
    public abstract class AbilityNode : ILogic
    {
        /// <summary>
        /// 所有的叶子节点的索引
        /// </summary>
        /// <returns></returns>
        public List<int> Childs = new();

        public AbilityBehavior Behavior { get; internal set; }
        public int Priority { get; internal set; }
        public int BehaviorIndex { get; internal set; }

        public void OnEnter()
        {

        }

        public void OnExit()
        {

        }

        public void OnInit()
        {

        }

        public void OnTick()
        {

        }
    }
}