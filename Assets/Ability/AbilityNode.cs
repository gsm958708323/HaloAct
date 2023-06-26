using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public abstract class AbilityNode : ILogic
    {
        /// <summary>
        /// 所有的叶子节点
        /// </summary>
        /// <returns></returns>
        public List<int> Childs = new();

        /// <summary>
        /// 动作列表
        /// </summary>
        /// <returns></returns>
        public List<AbilityAction> Actions = new();

        public AbilityBehavior behavior;

        public int FrameLength { get; internal set; }
        public bool IsLoop { get; internal set; }
        public string Name { get; internal set; }

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