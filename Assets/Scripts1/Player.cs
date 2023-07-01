using System;
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
        [HideInInspector] public CharacterController CharacterController;
        [HideInInspector] public Transform CameraTransform;
        [HideInInspector] public Animator Animator;

        PlayerGameInput Input;
        PlayerMovementStateMachine movementStateMachine;

        Vector3 checkGroundPos;
        [SerializeField] LayerMask groundLayer;
        bool isGround = false;

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
            isGround = CheckGround();
            movementStateMachine.PhysicsUpdate();
        }

        private bool CheckGround()
        {
            checkGroundPos.Set(transform.position.x, transform.position.y, transform.position.z);
            return Physics.CheckSphere(checkGroundPos, 0.2f, groundLayer, QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmos()
        {
            checkGroundPos.Set(transform.position.x, transform.position.y, transform.position.z);
            Gizmos.DrawWireSphere(checkGroundPos, 0.2f);
        }

        public PlayerInputActions GetPlayerAction()
        {
            return Input.PlayerAction.PlayerInput;
        }

        public bool GetIsGround()
        {
            return isGround;
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
