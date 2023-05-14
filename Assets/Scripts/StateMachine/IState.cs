using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementSystem
{
    public interface IState
    {
        public void Enter();
        public void Exit();
        public void Update();
        public void PhysicsUpdate();
        public void HandleInput();
    }

}
