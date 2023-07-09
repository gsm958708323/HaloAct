using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementSystem
{
    public class PlayerGroundedState : PlayerMovementState
    {
        public PlayerGroundedState(PlayerMovementStateMachine machine) : base(machine)
        {
        }

        public override void Enter()
    {
            base.Enter();
            SetAnimBool(AnimDef.Grouned, true);
        }

        public override void Exit()
        {
            SetAnimBool(AnimDef.Grouned, false);
            base.Exit();
        }

        protected override void AddInputCallbacks()
        {
            base.AddInputCallbacks();
            statemMachine.Player.GetPlayerAction().Jump.started += OnJump;
            statemMachine.Player.GetPlayerAction().Dash.started += OnDash;
        }



        protected override void RemoveInputCallbacks()
        {
            statemMachine.Player.GetPlayerAction().Jump.started -= OnJump;
            statemMachine.Player.GetPlayerAction().Dash.started -= OnDash;
            base.RemoveInputCallbacks();
        }

        private void OnDash(InputAction.CallbackContext context)
        {

        }

        private void OnJump(InputAction.CallbackContext context)
        {
            statemMachine.ChangeState(statemMachine.JumpState);
        }
    }
}
