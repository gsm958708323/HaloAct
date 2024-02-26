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
            var dir = Vector3.zero;
            if (model.Target != null)
            {
                dir = model.Target.Position - model.Position;
            }
            else
            {
                dir = model.Rotation * Vector3.forward;
            }

            if (dir == Vector3.zero)
                return;
            dir.y = 0;
            dir.Normalize();

            // 设置朝向
            var targetRot = Quaternion.LookRotation(dir);
            model.Rotation = Quaternion.Slerp(model.Rotation, targetRot, rotationRatio);

            // 设置位置偏移
            tree.ActorModel.Velocity.x = dir.x * moveSpeed;
            tree.ActorModel.Velocity.z = dir.z * moveSpeed;
        }
    }
}
