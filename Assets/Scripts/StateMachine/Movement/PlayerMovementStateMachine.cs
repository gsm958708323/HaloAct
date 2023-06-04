using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementSystem
{
    public class PlayerMovementStateMachine : StateMachine
    {
        public Player Player { get; }

        public PlayerIdlingState IdlingState { get; private set; }
        public PlayerMovingState MovingState { get; private set; }
        public PlayerStopingState StopingState { get; private set; }

        public PlayerLandingState LandingState { get; private set; }
        public PlayerJumpState JumpState { get; internal set; }
        public PlayerFallState FallState { get; internal set; }
        public PlayerDashState DashState { get; internal set; }

        public PlayerMovementStateMachine(Player player)
        {
            Player = player;
            IdlingState = new PlayerIdlingState(this);
            MovingState = new PlayerMovingState(this);
            StopingState = new PlayerStopingState(this);
            LandingState = new PlayerLandingState(this);
            FallState = new PlayerFallState(this);
            JumpState = new PlayerJumpState(this);
        }
    }
}

