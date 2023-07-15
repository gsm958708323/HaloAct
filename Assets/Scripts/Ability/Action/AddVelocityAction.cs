using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class AddVelocityAction : AbilityAction
    {
        public Vector3 Velocity;

        protected override void OnTick(int frame)
        {
            base.OnTick(frame);

            var transform = tree.ActorModel.transform;
            var add = transform.forward * Velocity.z + transform.right * Velocity.x + transform.up * Velocity.y;
            tree.ActorModel.Velocity += add;
        }
    }
}
