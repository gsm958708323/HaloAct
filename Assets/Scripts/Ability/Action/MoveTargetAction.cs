using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 先朝着目标移动，没有目标朝正前方移动
    /// </summary>
    public class MoveTargetAction : AbilityAction
    {
        public float moveSpeed = 10;
        public float rotationRatio = 0.2f;

        protected override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);

            // 设置朝向
            var model = tree.ActorModel;
            var trans1 = model.GetComp<TransfromComp>();
            if (trans1 is null)
            {
                return;
            }

            var dir = trans1.Rotation * Vector3.forward;
            if (model.Target != null)
            {
                var trans2 = model.Target.GetComp<TransfromComp>();
                if (trans2 != null)
                {
                    dir = trans2.Position - trans1.Position;
                }
            }

            if (dir == Vector3.zero)
                return;
            dir.y = 0;
            dir.Normalize();

            // 设置朝向
            var targetRot = Quaternion.LookRotation(dir);
            trans1.Rotation = Quaternion.Slerp(trans1.Rotation, targetRot, rotationRatio);

            // 设置位置偏移
            trans1.Velocity.x = dir.x * moveSpeed;
            trans1.Velocity.z = dir.z * moveSpeed;
        }
    }
}
