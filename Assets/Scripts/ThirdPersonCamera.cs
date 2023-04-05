using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float height = 2.0f;
    public float damping = 1.0f;
    public float rotationDamping = 10.0f;

    void LateUpdate()
    {
        if (!target) return;

        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, damping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        transform.position = target.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        Vector3 newPosition = new Vector3(transform.position.x, currentHeight, transform.position.z);
        transform.position = newPosition;

        transform.LookAt(target);
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            float rotation = Input.GetAxis("Mouse X") * rotationDamping;
            target.eulerAngles = new Vector3(0, target.eulerAngles.y + rotation, 0);
        }
    }
}
