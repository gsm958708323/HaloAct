using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;    //相机追随目标
    public float xSpeed = 200;  //X轴方向拖动速度
    public float ySpeed = 200;  //Y轴方向拖动速度
    public float mSpeed = 10;   //放大缩小速度
    public float yMinLimit = -50; //在Y轴最小移动范围
    public float yMaxLimit = 50; //在Y轴最大移动范围
    public float distance = 10;  //相机视角距离
    public float minDinstance = 2; //相机视角最小距离
    public float maxDinstance = 30; //相机视角最大距离
    public float x = 0.0f;
    public float y = 0.0f;
    public float damping = 5.0f;
    public bool needDamping = true;


    // Start is called before the first frame update
    void Start()
    {
        Vector3 angle = transform.eulerAngles;
        x = angle.y;
        y = angle.x;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target)
        {
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                y = ClamAngle(y, yMinLimit, yMaxLimit);

            }
            distance -= Input.GetAxis("Mouse ScrollWheel") * mSpeed;
            distance = Mathf.Clamp(distance, minDinstance, maxDinstance);
            Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
            Vector3 disVector = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * disVector + target.position;

            if (needDamping)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * damping);
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * damping);
            }
            else
            {
                transform.rotation = rotation;
                transform.position = position;
            }
        }
    }
    /// <summary>
    /// 限制某一轴移动范围
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    static float ClamAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }
}
