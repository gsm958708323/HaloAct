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

        public PlayerMovementStateMachine(Player player)
        {
            Player = player;
            IdlingState = new PlayerIdlingState(this);
            MovingState = new PlayerMovingState(this);
            StopingState = new PlayerStopingState(this);
            LandingState = new PlayerLandingState(this);
        }
    }
}

