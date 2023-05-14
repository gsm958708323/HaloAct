// using System.Linq;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace Test
// {
//     public class PlayerMovementController : ILogic<CharacterController, PlayerController>
//     {
//         Vector3 checkGroundPos;
//         [SerializeField] float checkGroundRadius;
//         [SerializeField] LayerMask groundLayer;
//         [SerializeField] float checkGroundOffsetY;

//         /// <summary>
//         /// 超过这个时间为大跳，否则为小跳
//         /// </summary>
//         [SerializeField] float inAirLevelTime = 0.15f;
//         float inAirTimer;

//         float gravity = 9.8f;
//         [SerializeField] float verticalVelocityMax = 20f;
//         /// <summary>
//         /// 当前的垂直速度
//         /// </summary>
//         [SerializeField] float verticalVelocity;
//         [SerializeField] float rotationAnglel;

//         Vector3 direction;
//         bool isGround;

//         // Update is called once per frame
//         public override void OnUpdate()
//         {
//             base.OnUpdate();

//             // 落地检测（球形检测 ）
//             bool isGround = CheckIsGround();
//             // print(isGround);
//             this.isGround = isGround;

//             // 小跳和大跳（通过落地时间区别）
//             if (isGround)
//             {
//                 inAirTimer = 0;
//                 verticalVelocity = 0;
//             }
//             else
//             {
//                 inAirTimer += Time.deltaTime;
//                 if (inAirTimer >= inAirLevelTime)
//                 {
//                     // 大跳动画
//                 }
//                 else
//                 {
//                     // 小跳动画
//                 }

//                 if (verticalVelocity < verticalVelocityMax)
//                 {
//                     verticalVelocity += Time.deltaTime * gravity;
//                 }
//             }

//             // 坡道检测

//             // 旋转
//             var Movement = GameInputMgr.Instance.Movement;
//             if (Movement != Vector2.zero)
//             {
//                 rotationAnglel = Mathf.Atan2(Movement.x, Movement.y) * Mathf.Rad2Deg;
//                 SetAngle(rotationAnglel);
//             }

//             // 重力计算
//             direction.Set(0, -verticalVelocity, 0);
//             MoveVertical(direction);
//         }

//         public void MoveHonrizontal(Vector3 direction)
//         {
//             ctrl.Move(direction * Time.deltaTime);
//         }

//         public void MoveVertical(Vector3 direction)
//         {
//             ctrl.Move(direction * Time.deltaTime);
//         }

//         public void SetAngle(float angle)
//         {
//             ctrl.transform.eulerAngles = Vector3.up * angle;
//         }

//         public bool GetIsGround()
//         {
//             return isGround;
//         }

//         private bool CheckIsGround()
//         {
//             checkGroundPos.Set(transform.position.x, transform.position.y - checkGroundOffsetY, transform.position.z);
//             return Physics.CheckSphere(checkGroundPos, checkGroundRadius, groundLayer, QueryTriggerInteraction.Ignore);
//         }

//         private void OnDrawGizmos()
//         {
//             checkGroundPos.Set(transform.position.x, transform.position.y - checkGroundOffsetY, transform.position.z);
//             Gizmos.DrawWireSphere(checkGroundPos, checkGroundRadius);
//         }
//     }
// }