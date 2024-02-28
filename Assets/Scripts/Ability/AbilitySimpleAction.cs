using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using System;

namespace Ability
{
    /// <summary>
    /// 简单行为，一帧执行
    /// </summary>
    [Serializable]
    public abstract class AbilitySimpleAction : IAbilityAction
    {
        protected AbilityBehaviorTree tree;

        public int StartFrame;

        public void Enter(AbilityBehaviorTree tree)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.AbilityAction);
            this.tree = tree;
            OnEnter();
        }

        protected virtual void OnEnter()
        {
        }
    }
}

