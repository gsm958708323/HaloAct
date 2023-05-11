using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class PlayerMovementController : ILogic
    {
        CharacterController characterController;

        Vector3 checkGroundPos;
        [SerializeField] float checkGroundRadius;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float checkGroundOffsetY;

        /// <summary>
        /// 超过这个时间为大跳，否则为小跳
        /// </summary>
        [SerializeField] float inAirLevelTime = 0.15f;
        float inAirTimer;

        float gravity = 9.8f;
        [SerializeField] float verticalVelocityMax = 20f;
        /// <summary>
        /// 当前的垂直速度
        /// </summary>
        [SerializeField] float verticalVelocity;
        Vector3 direction;

        public override void Bind<T>(T t)
        {
            base.Bind(t);
            this.characterController = t as CharacterController;
        }

        // Update is called once per frame
        public override void OnUpdate()
        {
            base.OnUpdate();

            // 落地检测（球形检测 ）
            bool isGround = CheckIsGround();
            print(isGround);

            // 小跳和大跳（通过落地时间区别）
            if (isGround)
            {
                inAirTimer = 0;
                verticalVelocity = 0;
            }
            else
            {
                inAirTimer += Time.deltaTime;
                if (inAirTimer >= inAirLevelTime)
                {
                    // 大跳动画
                }
                else
                {
                    // 小跳动画
                }

                if (verticalVelocity < verticalVelocityMax)
                {
                    verticalVelocity += Time.deltaTime * gravity;
                }
            }

            // 坡道检测


            // 重力计算
            direction.Set(0, -verticalVelocity, 0);
            characterController.Move(direction * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            checkGroundPos.Set(transform.position.x, transform.position.y - checkGroundOffsetY, transform.position.z);
            Gizmos.DrawWireSphere(checkGroundPos, checkGroundRadius);
        }

        private bool CheckIsGround()
        {
            checkGroundPos.Set(transform.position.x, transform.position.y - checkGroundOffsetY, transform.position.z);
            return Physics.CheckSphere(checkGroundPos, checkGroundRadius, groundLayer, QueryTriggerInteraction.Ignore);
        }
    }
}