using System;
using System.Collections;
using System.Collections.Generic;
using MovementSystem;
using UnityEngine;

public class CameraMgr : MonoBehaviour
{
    public PlayerGameInput input;

    Vector3 targetCameraEulers;
    Vector3 curCameraEulers;
    Vector3 dampVelocity;
    [SerializeField] float cameraRotSpeed = 1;
    /// <summary>
    /// 平滑过渡的时间
    /// </summary>
    [SerializeField] float cameraSmoothTime = 0.5f;
    /// <summary>
    /// 摄像机垂直方向最大旋转角度
    /// </summary>
    [SerializeField] Vector2 maxVerticalAngle = new Vector3(-30, 30);
    /// <summary>
    /// 视野缩放
    /// </summary>
    [SerializeField] float cameraZoom = 1f;
    /// <summary>
    /// 相机视野偏移
    /// </summary>
    [SerializeField] Vector3 cameraOffset;

    [SerializeField] float posSmoothTime = 0.5f;

    Vector2 cameraLook;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        Rotate();
    }

    void HandleInput()
    {
        cameraLook = input.PlayerAction.PlayerInput.CameraLook.ReadValue<Vector2>();
    }

    private void Rotate()
    {
        //设置旋转(摄像机水平方向的旋转是围绕世界Y轴)
        targetCameraEulers.y += cameraLook.x * cameraRotSpeed;
        targetCameraEulers.x -= cameraLook.y * cameraRotSpeed;
        targetCameraEulers.x = Mathf.Clamp(targetCameraEulers.x, maxVerticalAngle.x, maxVerticalAngle.y);

        curCameraEulers = Vector3.SmoothDamp(curCameraEulers, new Vector3(targetCameraEulers.x, targetCameraEulers.y, 0), ref dampVelocity, cameraSmoothTime);
        // 设置旋转（欧拉角没有大小和方向概念），表示各个轴向上的旋转角度
        transform.eulerAngles = curCameraEulers;

        // 设置位置（将原始向量偏移）
        var pos = input.transform.position + (transform.forward * cameraOffset.z + transform.right * cameraOffset.x + transform.up * cameraOffset.y) * cameraZoom;
        transform.position = pos;
        transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * posSmoothTime);
    }
}
