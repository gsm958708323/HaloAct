using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 受击配置
    /// </summary> <summary>
    /// </summary>
    public class AbilityUnderAttack : ILogicT<ActorModel>
    {
        public virtual void Init()
        {

        }

        public virtual void Enter(ActorModel model)
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void Tick(int frame)
        {
        }

        internal void OnHurt(ActorModel atkModel, ActorModel model, AbilityAttack atk, Transform atkTrans)
        {
            throw new NotImplementedException();
        }
    }
}

