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
    public class AbilityUnderAttack
    {
        internal void OnHurt(ActorModel atkModel, ActorModel model, AbilityBehaviorAttack atk, Transform atkTrans)
        {
            Debugger.Log($"OnHurt {GetType()}", LogDomain.AbilityUnderAttack);

        }
    }
}

