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
        [HideInInspector]public CharacterController CharacterController;
        [HideInInspector]public Transform CameraTransform;
        [HideInInspector]public Animator Animator;

        PlayerGameInput Input;
        PlayerMovementStateMachine movementStateMachine;

        private void Awake()
        {
            Input = GetComponent<PlayerGameInput>();
            CharacterController = GetComponent<CharacterController>();
            Animator = GetComponent<Animator>();
            CameraTransform = Camera.main.transform;
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

        public void Move(Vector3 dir)
        {
            CharacterController.Move(dir);
        }

        public void SetDir(Vector3 dir)
        {
            transform.eulerAngles = dir;
        }
    }
}
