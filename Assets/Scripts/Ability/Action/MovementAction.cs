using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class MovementAction : AbilityAction
    {
        public float moveSpeed = 10;

        public override void Tick(int frame)
        {
            base.Tick(frame);

            var inputDir = tree.ActorModel.GetPlayerInput().Movement.ReadValue<Vector2>();
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
                tree.ActorModel.RotationAngle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            }
        }
    }
}
