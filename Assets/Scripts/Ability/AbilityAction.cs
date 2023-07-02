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
            Debugger.Log($"Enter {GetType()}", LogDomain.AbilityAction);
            this.tree = tree;
        }

        public virtual void Exit()
        {
            Debugger.Log($"Exit {GetType()}", LogDomain.AbilityAction);
            this.tree = null;
        }
        
        public virtual void Tick(int frame)
        {
            
        }
    }
}

