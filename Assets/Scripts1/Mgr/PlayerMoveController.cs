using System.Collections;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{ float h; //水平轴系数
    float v; //垂直轴系数
    public float speed = 6;//速度
    public float turnSpeed = 10;//旋转速度
    public Transform camTransform; //相机
    Vector3 camForward; //临时三维坐标

    private void Start() {
        camTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    void Move()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        transform.Translate(camTransform.right * h * speed * Time.deltaTime + camForward * v * speed * Time.deltaTime , Space.World);
        //水平垂直方向系数不为0表示需要进行旋转
        if (h != 0 || v != 0)
        {
            Rotating(h, v);
        }
    }
    //旋转
    void Rotating(float hh, float vv)
    {
        camForward = Vector3.Cross(camTransform.right, Vector3.up);
        Vector3 targetDir = camTransform.right * hh + camForward * vv;
        Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}