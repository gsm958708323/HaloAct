using UnityEngine;

namespace MovementSystem
{
    public class PlayerIdlingState : PlayerGroundedState
    {
        public PlayerIdlingState(PlayerMovementStateMachine machine) : base(machine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            SetAnimBool(AnimDef.IsIdling, true);
        }

        public override void Exit()
        {
            SetAnimBool(AnimDef.IsIdling, false);

            base.Exit();
        }

        public override void Update()
        {
            base.Update();
            if (movementInput == Vector2.zero)
            {
                return;
            }   

            statemMachine.ChangeState(statemMachine.MovingState);
        }
    }
}