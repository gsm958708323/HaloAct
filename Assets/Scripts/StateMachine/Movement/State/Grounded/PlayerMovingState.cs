using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementSystem
{
    public class PlayerMovingState : PlayerGroundedState
    {
        bool walkToggle = false;
        float moveValue;

        public PlayerMovingState(PlayerMovementStateMachine machine) : base(machine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            SetAnimBool(AnimDef.IsMoving, true);
        }

        public override void Exit()
        {
            SetAnimBool(AnimDef.IsMoving, false);

            base.Exit();
        }

        public override void Update()
        {
            base.Update();
            if (movementInput == Vector2.zero)
            {
                statemMachine.ChangeState(statemMachine.StopingState);
            }
        }

        protected override void AddInputCallbacks()
        {
            base.AddInputCallbacks();

            statemMachine.Player.GetPlayerAction().Spint.performed += OnSprint;
            statemMachine.Player.GetPlayerAction().Dash.performed += OnDash;
            statemMachine.Player.GetPlayerAction().WalkToggle.performed += OnWolkToggle;
        }


        protected override void RemoveInputCallbacks()
        {
            statemMachine.Player.GetPlayerAction().Spint.performed -= OnSprint;
            statemMachine.Player.GetPlayerAction().Dash.performed -= OnDash;
            statemMachine.Player.GetPlayerAction().WalkToggle.performed -= OnWolkToggle;

            base.RemoveInputCallbacks();
        }


        private void OnWolkToggle(InputAction.CallbackContext context)
        {
            walkToggle = !walkToggle;
            moveValue = walkToggle ? 1f : 0f;
            speedModifier = walkToggle ? 2f : 1f;
            SetAnimFloat(AnimDef.MoveValue, moveValue);
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            
        }

        private void OnSprint(InputAction.CallbackContext context)
        {

        }
    }
}