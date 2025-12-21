using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 不为0，则重置对应速度的分量
    /// </summary>
    public class ResetVelocityAction : AbilityAction
    {
        public Vector3 Reset;

        protected override void OnEnter()
        {
            base.OnEnter();

            var transComp = tree.ActorModel.GetComp<TransfromComp>();
            if (Reset.x != 0)
            {
                transComp.Velocity.x = 0;
            }

            if (Reset.y != 0)
            {
                transComp.Velocity.y = 0;
            }

            if (Reset.z != 0)
            {
                transComp.Velocity.z = 0;
            }
        }
    }
}

