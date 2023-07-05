using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class HurtBox : AbilityBox
    {
        public Vector3 HitPoint { get; internal set; }

        /// <summary>
        /// 被攻击，传入攻击者数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transform"></param>
        /// <param name="atk"></param>
        internal void OnHurt(ActorModel model, Transform transform, AbilityAttack atk)
        {
            throw new NotImplementedException();
        }
    }
}
