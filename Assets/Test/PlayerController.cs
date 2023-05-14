// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Test;
// using UnityEngine;

// namespace Test
// {
//     public class PlayerController : MonoBehaviour
//     {
//         [HideInInspector] public PlayerMovementController PlayerMovementController;
//         [HideInInspector] public PlayerAnimatorController PlayerAnimatorController;

//         private void Awake()
//         {
//             PlayerMovementController = gameObject.AddComponent<PlayerMovementController>();
//             PlayerAnimatorController = gameObject.AddComponent<PlayerAnimatorController>();

//             PlayerAnimatorController.Bind(GetComponent<Animator>(), this);
//             PlayerMovementController.Bind(GetComponent<CharacterController>(), this);
//         }

//         private void Start()
//         {
//             PlayerAnimatorController.OnStart();
//             PlayerMovementController.OnStart();
//         }

//         private void Update()
//         {
//             PlayerAnimatorController.OnUpdate();
//             PlayerMovementController.OnUpdate();
//         }

//         private void OnAnimatorMove()
//         {
//             PlayerAnimatorController.OnAnimatorMove();
//         }
//     }
// }

