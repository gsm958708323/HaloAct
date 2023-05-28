using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementSystem
{
    public class PlayerGroundedState : PlayerMovementState
    {
        public PlayerGroundedState(PlayerMovementStateMachine machine) : base(machine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            SetAnimBool(AnimDef.Grouned, true);
        }

        public override void Exit()
        {
            SetAnimBool(AnimDef.Grouned, false);
            base.Exit();
        }
    }
}
