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

        protected override void OnTick(int frame)
        {
            base.OnTick(frame);
            var inputDir = tree.ActorModel.GameInput.GetPlayerInput().Movement.ReadValue<Vector2>();
            Vector3 moveDir = Vector3.zero;
            if (inputDir != Vector2.zero)
            {
                var camera = Camera.main.transform;
                moveDir = camera.forward * inputDir.y + camera.right * inputDir.x;
            }

            tree.ActorModel.InputDir = inputDir;
            tree.ActorModel.Velocity.x = moveDir.x * moveSpeed;
            tree.ActorModel.Velocity.z = moveDir.z * moveSpeed;

            if (tree.ActorModel.Velocity != Vector3.zero)
            {
                Debug.Log($"{11111111111}{inputDir} {tree.ActorModel.Velocity}");
            }

            Rotation(inputDir);
        }

        private void Rotation(Vector2 inputDir)
        {
            if (inputDir != Vector2.zero)
            {
                var angle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

                var targetRotation = Quaternion.Euler(0, angle, 0);
                tree.ActorModel.Rotation = Quaternion.Slerp(tree.ActorModel.Rotation, targetRotation, rotationRatio);
            }
        }
    }
}
