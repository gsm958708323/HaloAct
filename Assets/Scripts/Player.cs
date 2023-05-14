using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameInput;

namespace MovementSystem
{
    [RequireComponent(typeof(PlayerGameInput))]
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        PlayerGameInput Input;
        public CharacterController CharacterController;

        PlayerMovementStateMachine movementStateMachine;

        private void Awake()
        {
            Input = GetComponent<PlayerGameInput>();
            CharacterController = GetComponent<CharacterController>();
            movementStateMachine = new PlayerMovementStateMachine(this);
        }

        void Start()
        {
            movementStateMachine.ChangeState(movementStateMachine.IdlingState);
        }

        void Update()
        {
            movementStateMachine.HandleInput();
            movementStateMachine.Update();
        }

        private void FixedUpdate()
        {
            movementStateMachine.PhysicsUpdate();
        }

        public PlayerInputActions GetPlayerAction()
        {
            return Input.PlayerAction.PlayerInput;
        }
    }
}
