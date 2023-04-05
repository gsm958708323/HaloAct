using System.Collections;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    Rigidbody rigi;
    public int speed = 10;

    private void Start()
    {
        rigi = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        rigi.AddForce(transform.forward * v * speed + transform.right * h * speed, ForceMode.Force);
    }
}