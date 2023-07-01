using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;

namespace Ability
{
    public abstract class AbilityAction : ILogicT<AbilityBehaviorTree>
    {
        public int StartFrame = 1;
        public int EndFrame = 60;

        public virtual void Enter(AbilityBehaviorTree t)
        {
        }

        public virtual void Exit(AbilityBehaviorTree t)
        {
        }

        public virtual void Init(AbilityBehaviorTree t)
        {
        }

        public virtual void Tick(AbilityBehaviorTree t)
        {
        }
    }
}

