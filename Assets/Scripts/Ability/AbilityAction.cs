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

        public virtual void Enter(AbilityBehaviorTree t)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.AbilityAction);
        }

        public virtual void Exit(AbilityBehaviorTree t)
        {
            Debugger.Log($"Exit {GetType()}", LogDomain.AbilityAction);
        }

        public virtual void Init(AbilityBehaviorTree t)
        {
        }

        public virtual void Tick(AbilityBehaviorTree t)
        {
        }
    }
}

