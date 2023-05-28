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

        protected override void AddInputCallbacks()
        {
            base.AddInputCallbacks();
            statemMachine.Player.GetPlayerAction().Dash.started += OnDash;
            statemMachine.Player.GetPlayerAction().Jump.started += OnJump;
        }
        protected override void RemoveInputCallbacks()
        {
            statemMachine.Player.GetPlayerAction().Dash.started -= OnDash;
            statemMachine.Player.GetPlayerAction().Jump.started -= OnJump;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            statemMachine.ChangeState(statemMachine.JumpState);
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            statemMachine.ChangeState(statemMachine.DashState);
        }
    }
}
