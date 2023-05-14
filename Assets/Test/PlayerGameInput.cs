using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameInput : MonoBehaviour
{
    public GameInput PlayerAction;

    private void Awake()
    {
        PlayerAction = new GameInput();
    }

    private void OnEnable()
    {
        PlayerAction?.Enable();
    }

    private void OnDisable()
    {
        PlayerAction?.Disable();
    }
}

