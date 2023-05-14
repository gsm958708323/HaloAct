using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class GameInputMgr : MonoSingleton<GameInputMgr>
    {
        GameInput gameInput;
        public Vector2 Movement => gameInput.PlayerInput.Movement.ReadValue<Vector2>();
        public Vector2 CameraLook => gameInput.PlayerInput.CameraLook.ReadValue<Vector2>();
        public bool IsRun => gameInput.PlayerInput.Run.triggered;
        public bool HasInput => Movement != Vector2.zero;

        private void Awake()
        {
            gameInput = new GameInput();
        }

        private void OnEnable()
        {
            gameInput?.Enable();
        }

        private void OnDisable()
        {
            gameInput?.Disable();
        }
    }
}
