using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameInput;

namespace MovementSystem
{
    [RequireComponent(typeof(PlayerGameInput))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [HideInInspector] public CharacterController CharacterController;
        [HideInInspector] public Transform CameraTransform;
        [HideInInspector] public Animator Animator;

        PlayerGameInput Input;
        PlayerMovementStateMachine movementStateMachine;

        Vector3 checkGroundPos;
        [SerializeField] LayerMask groundLayer;
        bool isGround = false;
        /// <summary>
        /// x为检测半径，y为偏移量
        /// </summary>
        public Vector2 CheckGroundSetting;

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
            checkGroundPos.Set(transform.position.x, transform.position.y + CheckGroundSetting.y, transform.position.z);
            return Physics.CheckSphere(checkGroundPos, CheckGroundSetting.x, groundLayer, QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmos()
        {
            checkGroundPos.Set(transform.position.x, transform.position.y + CheckGroundSetting.y, transform.position.z);
            Gizmos.DrawWireSphere(checkGroundPos, CheckGroundSetting.x);
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
