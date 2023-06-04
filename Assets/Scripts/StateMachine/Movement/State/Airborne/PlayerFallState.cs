namespace MovementSystem
{
    public class PlayerFallState : PlayerAirborneState
    {
        public PlayerFallState(PlayerMovementStateMachine machine) : base(machine)
        {
        }
        public override void Enter()
        {
            base.Enter();
            SetAnimBool(AnimDef.IsFalling, true);
        }

        public override void Exit()
        {
            SetAnimBool(AnimDef.IsFalling, false);
            base.Exit();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (VerticalInput == 0)
            {
                statemMachine.ChangeState(statemMachine.IdlingState);
            }
        }
    }
}