using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public abstract class AbilityAction : ILogicT<ActorModel>
    {
        public int StartFrame = 1;
        public int EndFrame = 60;

        public virtual void Enter(ActorModel t)
        {
        }

        public virtual void Exit(ActorModel t)
        {
        }

        public virtual void Init(ActorModel t)
        {
        }

        public virtual void Tick(ActorModel t)
        {
        }
    }
}

