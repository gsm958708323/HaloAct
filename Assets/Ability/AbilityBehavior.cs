using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 存储行为数据：定义此行为将要执行的动作
    /// </summary>
    public abstract class AbilityBehavior : ILogic
    {
        /// <summary>
        /// 动作列表
        /// </summary>
        /// <returns></returns>
        public List<AbilityAction> Actions = new();

        public int FrameLength { get; internal set; }
        public bool IsLoop { get; internal set; }
        public string Name { get; internal set; }
        public KeyCode InputKey { get; internal set; }

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

        internal bool CheckCondition(ActorModel actorModel)
        {
            throw new NotImplementedException();
        }
    }
}