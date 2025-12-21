using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class AddVelocityAction : AbilityAction
    {
        public Vector3 Velocity;

        protected override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);

            var transformComp = tree.ActorModel.GetComp<TransfromComp>();
            var add = transformComp.forward * Velocity.z + transformComp.right * Velocity.x + transformComp.up * Velocity.y;
            transformComp.Velocity += add;
        }
    }
}
