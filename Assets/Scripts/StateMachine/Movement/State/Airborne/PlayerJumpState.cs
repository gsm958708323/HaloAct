using UnityEngine;
using System;

namespace MovementSystem
{
    public class PlayerJumpState : PlayerAirborneState
    {
        public PlayerJumpState(PlayerMovementStateMachine machine) : base(machine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Jump();
        }

        public override void Exit()
        {
            base.Exit();
        }

        private void Jump()
        {
            VerticalInput = 5;
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (VerticalInput < 0)
            {
                statemMachine.ChangeState(statemMachine.FallState);
            }
        }
    }
}