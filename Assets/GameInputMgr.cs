using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInputMgr : MonoSingleton<GameInputMgr>
{
    GameInput gameInput;
    public Vector3 Movement => gameInput.PlayerInput.Movement.ReadValue<Vector2>();
    public Vector3 CameraLook => gameInput.PlayerInput.CameraLook.ReadValue<Vector2>();

    private void Awake()
    {
        gameInput = new GameInput();
    }

    // private void Update() {
    //     print(Movement);
    //     print(CameraLook);
    // }

    private void OnEnable()
    {
        gameInput?.Enable();
    }

    private void OnDisable()
    {
        gameInput?.Disable();
    }
}
