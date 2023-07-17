using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class MovementAction : AbilityAction
    {
        public float moveSpeed = 10;
        public float rotationTime = 0.1f;
        private float currentVelocity;

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
            tree.ActorModel.Velocity = new Vector3(moveDir.x * moveSpeed, tree.ActorModel.Velocity.y, moveDir.z * moveSpeed);

            Rotation(inputDir);
        }

        private void Rotation(Vector2 inputDir)
        {
            if (inputDir != Vector2.zero)
            {
                var angle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                tree.ActorModel.EulerAngles = Vector3.up * Mathf.SmoothDampAngle(tree.ActorModel.EulerAngles.y, angle, ref currentVelocity, rotationTime);
                tree.ActorModel.EulerAngles.y %= 360;
            }
        }
    }
}
