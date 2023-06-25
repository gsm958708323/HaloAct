using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public abstract class AbilityAction : ILogicT<ActorModel>
    {
        public virtual void OnEnter(ActorModel t)
        {
        }

        public virtual void OnExit(ActorModel t)
        {
        }

        public virtual void OnInit(ActorModel t)
        {
        }

        public virtual void OnTick(ActorModel t)
        {
        }
    }
}

