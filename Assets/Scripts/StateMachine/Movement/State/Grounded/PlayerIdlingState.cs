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

            SetAnimBool(AnimDef.Idle, true);
        }

        public override void Exit()
        {
            base.Exit();

            SetAnimBool(AnimDef.Idle, false);
        }
    }
}