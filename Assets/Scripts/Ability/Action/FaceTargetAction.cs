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
            var trans1 = model.GetComp<TransfromComp>();
            var trans2 = model.Target.GetComp<TransfromComp>();
            if (trans1 == null || trans2 == null)
            {
                return;
            }

            var dir = trans2.Position - trans1.Position;
            dir.y = 0;
            var targetRot = Quaternion.LookRotation(dir);
            trans1.Rotation = Quaternion.Slerp(trans1.Rotation, targetRot, rotationRatio);
        }
    }
}


