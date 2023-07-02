using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using System;

namespace Ability
{
    public abstract class AbilityAction : ILogicT<AbilityBehaviorTree>
    {
        public int StartFrame = 1;
        public int EndFrame = 60;
        protected AbilityBehaviorTree tree;

        public virtual void Init()
        {
        }

        public virtual void Enter(AbilityBehaviorTree tree)
        {
            this.tree = tree;
            Debugger.Log($"Enter {GetType()}", LogDomain.AbilityAction);
        }

        public virtual void Exit()
        {
            this.tree = null;
            Debugger.Log($"Exit {GetType()}", LogDomain.AbilityAction);
        }

        public virtual void Tick()
        {
        }
    }
}

