using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class MovementAction : AbilityAction
    {
        public float moveSpeed = 10;
        public float rotationRatio = 0.2f;
        VelocityComp velocityComp;
        TransfromComp transComp;

        protected override void OnEnter()
        {
            base.OnEnter();
            velocityComp = tree.Entity.GetComp<VelocityComp>();
            transComp = tree.Entity.GetComp<TransfromComp>();
        }

        protected override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);
            var inputDir = FightManager.GameInput.GetPlayerInput().Movement.ReadValue<Vector2>();
            Vector3 moveDir = Vector3.zero;
            if (inputDir != Vector2.zero)
            {
                var camera = Camera.main.transform;
                moveDir = camera.forward * inputDir.y + camera.right * inputDir.x;
            }

            velocityComp.Velocity.x = moveDir.x * moveSpeed;
            velocityComp.Velocity.z = moveDir.z * moveSpeed;

            Rotation(inputDir);
        }

        private void Rotation(Vector2 inputDir)
        {
            if (inputDir != Vector2.zero)
            {
                var angle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

                var targetRotation = Quaternion.Euler(0, angle, 0);
                transComp.Rotation = Quaternion.Slerp(transComp.Rotation, targetRotation, rotationRatio);
            }
        }
    }
}
