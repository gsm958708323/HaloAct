namespace MovementSystem
{
    public class PlayerStopingState : PlayerGroundedState
    {
        public PlayerStopingState(PlayerMovementStateMachine machine) : base(machine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            SetAnimBool(AnimDef.IsStoping, true);
        }

        public override void Exit()
        {
            SetAnimBool(AnimDef.IsStoping, false);

            base.Exit();
        }

        public override void OnAnimationFinished()
        {
            base.OnAnimationFinished();

            statemMachine.ChangeState(statemMachine.IdlingState);
        }
    }
}