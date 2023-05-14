// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace Test
// {
//     public static class AnimtorDefine
//     {
//         public const string Look = "Look";
//         public const string Movement = "Movement";
//         public const string HasInput = "HasInput";
//         public const string Run = "Run";
//     }

//     public class PlayerAnimatorController : ILogic<Animator, PlayerController>
//     {
//         float tweenTime = 0.2f;

//         //应用根运动
//         internal void OnAnimatorMove()
//         {
//             ctrl.ApplyBuiltinRootMotion();
//             parent.PlayerMovementController.MoveHonrizontal(ctrl.deltaPosition);
//         }

//         public override void OnStart()
//         {
//             base.OnStart();
//         }

//         public override void OnUpdate()
//         {
//             base.OnUpdate();

//             UpdateLocomotion();
//         }

//         private void UpdateLocomotion()
//         {
//             bool hasInput = GameInputMgr.Instance.Movement != Vector2.zero;
//             ctrl.SetBool(AnimtorDefine.HasInput, hasInput);

//             bool isRun = GameInputMgr.Instance.IsRun;
//             ctrl.SetBool(AnimtorDefine.Run, isRun);

//             bool isGround = parent.PlayerMovementController.GetIsGround();
//             if (isGround && hasInput)
//             {
//                 //判断是否按下快速奔跑
//                 float speed = isRun ? GameInputMgr.Instance.Movement.sqrMagnitude * 2 : GameInputMgr.Instance.Movement.sqrMagnitude;
//                 ctrl.SetFloat(AnimtorDefine.Movement, speed, tweenTime, Time.deltaTime);
//             }
//             else
//             {
//                 ctrl.SetFloat(AnimtorDefine.Movement, 0, tweenTime, Time.deltaTime);
//             }
//         }
//     }
// }