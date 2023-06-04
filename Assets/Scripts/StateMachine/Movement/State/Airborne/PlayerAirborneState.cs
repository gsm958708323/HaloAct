using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementSystem
{
    public class PlayerAirborneState : PlayerMovementState
    {
        float fallTime;
        public PlayerAirborneState(PlayerMovementStateMachine machine) : base(machine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            fallTime = 0;
            
            SetAnimBool(AnimDef.Airborne, true);
        }

        public override void Exit()
        {
            SetAnimBool(AnimDef.Airborne, false);
            base.Exit();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            var isGround = statemMachine.Player.GetIsGround();
            //等过了最小时间才检查是否落地
            if (isGround && fallTime >= 0.1f)
            {
                VerticalInput = 0;
                fallTime = 0;
            }
            else
            {
                fallTime += Time.deltaTime;
                VerticalInput -= Time.deltaTime * 9.8f;
            }

            // Debug.LogWarning($"fallTime:{fallTime} isGround:{isGround} VerticalInput:{VerticalInput}");
        }
    }
}
