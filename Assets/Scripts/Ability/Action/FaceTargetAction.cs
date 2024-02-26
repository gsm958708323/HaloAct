using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ability
{
    public class FaceTargetAction : AbilityAction
    {
        public float rotationRatio = 0.2f;

        protected override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);

            var model = tree.ActorModel;
            if (model.Target == null) return;

            var dir = model.Target.Position - model.Position;
            dir.y = 0;
            var targetRot = Quaternion.LookRotation(dir);
            model.Rotation = Quaternion.Slerp(model.Rotation, targetRot, rotationRatio);
        }
    }
}


