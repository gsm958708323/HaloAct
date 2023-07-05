using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class AbilityBox : MonoBehaviour, ILogicT<ActorModel>
    {
        protected ActorModel model;
        public void Init()
        {

        }

        public void Enter(ActorModel model)
        {
            this.model = model;
        }

        public void Exit()
        {
            this.model = null;
        }

        public void Tick(int frame)
        {

        }
    }
}
