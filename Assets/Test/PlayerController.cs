using System;
using System.Collections;
using System.Collections.Generic;
using Test;
using UnityEngine;

namespace Tset
{
    public class PlayerController : MonoBehaviour
    {
        [HideInInspector] public PlayerMovementController PlayerMovementController;
        [HideInInspector] public PlayerAnimatorController PlayerAnimatorController;

        private void Awake()
        {
            PlayerMovementController = gameObject.AddComponent<PlayerMovementController>();
            PlayerAnimatorController = gameObject.AddComponent<PlayerAnimatorController>();

            PlayerMovementController.Bind<CharacterController>(GetComponent<CharacterController>());
            PlayerAnimatorController.Bind<Animator>(GetComponent<Animator>());
        }

        private void Start()
        {
            PlayerMovementController.OnStart();
            PlayerAnimatorController.OnStart();
        }

        private void Update()
        {
            PlayerMovementController.OnUpdate();
            PlayerAnimatorController.OnUpdate();
        }
    }
}

