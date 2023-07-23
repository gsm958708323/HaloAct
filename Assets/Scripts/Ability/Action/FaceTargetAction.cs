using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ability
{
    public class FaceTargetAction : AbilityAction
    {
        public float rotationRatio = 0.2f;

        protected override void OnTick(int frame)
        {
            base.OnTick(frame);

            var model = tree.ActorModel;
            if (model.Target == null) return;

            var dir = model.Target.transform.position - model.transform.position;
            dir.y = 0;
            var targetRot = Quaternion.LookRotation(dir);
            model.Rotation = Quaternion.Slerp(model.Rotation, targetRot, rotationRatio);
        }
    }
}


