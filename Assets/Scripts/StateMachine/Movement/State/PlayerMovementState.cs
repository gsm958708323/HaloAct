using UnityEngine;
namespace MovementSystem
{
    public class PlayerMovementState : IState
    {
        protected PlayerMovementStateMachine statemMachine;
        Vector2 movementInput;
        float baseSpeed = 1;
        float speedModifier = 1;
        float speedTemp = 0.1f;

        public PlayerMovementState(PlayerMovementStateMachine machine)
        {
            statemMachine = machine;
        }

        public virtual void Enter()
        {
            Debug.Log($"State Enter: {GetType().Name}");
        }

        public virtual void Exit()
        {
        }

        public virtual void HandleInput()
        {
            movementInput = statemMachine.Player.GetPlayerAction().Movement.ReadValue<Vector2>();
        }

        public virtual void PhysicsUpdate()
        {
            if (movementInput == Vector2.zero || speedModifier == 0)
            {
                return;
            }

            Vector3 dir = new Vector3(movementInput.x, 0, movementInput.y);
            float speed = baseSpeed * speedModifier;

            statemMachine.Player.CharacterController.Move(dir * speed * speedTemp);
        }

        public virtual void Update()
        {
        }
    }
}