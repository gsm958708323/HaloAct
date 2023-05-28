using System;
using UnityEngine;
namespace MovementSystem
{
    public class PlayerMovementState : IState
    {
        protected PlayerMovementStateMachine statemMachine;
        protected Vector2 movementInput;
        float baseSpeed = 1;
        float speedModifier = 1;
        float speedTemp = 0.1f;

        public PlayerMovementState(PlayerMovementStateMachine machine)
        {
            statemMachine = machine;
        }

        public virtual void Enter()
        {
            Debug.Log("Entered " + GetType().Name + " state.");
            AddInputCallbacks();
        }

        public virtual void Exit()
        {
            Debug.Log("Exited " + GetType().Name + " state.");
            RemoveInputCallbacks();
        }

        protected virtual void AddInputCallbacks()
        {
        }


        protected virtual void RemoveInputCallbacks()
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

            Vector3 dir = GetInputDirection();
            float angle = Rotate(dir);
            Vector3 rotationDir = GetRotationDirection(angle);

            float speed = baseSpeed * speedModifier;
            statemMachine.Player.Move(rotationDir * speed * speedTemp);
        }

        protected void SetAnimBool(string name, bool toggle)
        {
            statemMachine.Player.Animator.SetBool(name, toggle);
        }

        protected void SetAnimFloat(string name, float value)
        {
            statemMachine.Player.Animator.SetFloat(name, value);
        }

        /// <summary>
        /// 获取方向向量
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Vector3 GetRotationDirection(float angle)
        {
            // 四元数与向量相乘：把指向z轴方向的向量做旋转
            return Quaternion.Euler(0, angle, 0) * Vector3.forward;
        }

        /// <summary>
        /// 获取旋转角度
        /// </summary>
        /// <param name="dir"></param>
        private float Rotate(Vector3 dir)
        {
            // 获取输入的朝向
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // 增加摄像机的旋转角度
            targetAngle += statemMachine.Player.CameraTransform.eulerAngles.y;
            targetAngle = targetAngle % 360;

            statemMachine.Player.SetDir(Vector3.up * targetAngle);
            return targetAngle;
        }

        /// <summary>
        /// 获取输入方向
        /// </summary>
        /// <returns></returns>
        public virtual Vector3 GetInputDirection()
        {
            return new Vector3(movementInput.x, 0, movementInput.y);
        }

        public virtual void Update()
        {
            Animator animator = statemMachine.Player.Animator;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // 如果动画已完成
            if (stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0))
            {
                // 触发回调
                OnAnimationFinished();
            }
        }

        public virtual void OnAnimationFinished()
        {
        }
    }
}