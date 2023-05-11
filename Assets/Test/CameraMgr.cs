using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class CameraMgr : MonoBehaviour
    {
        public Transform Target;

        Vector3 targetCameraRot;
        Vector3 curCameraRot;
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
        [SerializeField] float cameraTargetOffset = 1f;

        [SerializeField] float posSmoothTime = 0.5f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //设置旋转(摄像机水平方向的旋转是围绕世界Y轴)
            targetCameraRot.y += GameInputMgr.Instance.CameraLook.x * cameraRotSpeed;
            targetCameraRot.x -= GameInputMgr.Instance.CameraLook.y * cameraRotSpeed;
            targetCameraRot.x = Mathf.Clamp(targetCameraRot.x, maxVerticalAngle.x, maxVerticalAngle.y);
        }

        private void LateUpdate()
        {
            curCameraRot = Vector3.SmoothDamp(curCameraRot, new Vector3(targetCameraRot.x, targetCameraRot.y, 0), ref dampVelocity, cameraSmoothTime);
            // gsm todo
            transform.eulerAngles = curCameraRot;

            //设置位置
            var pos = Target.position + (-transform.forward * cameraTargetOffset);
            transform.position = pos;
            // transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * posSmoothTime);
        }
    }
}