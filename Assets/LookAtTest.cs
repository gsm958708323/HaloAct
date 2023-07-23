using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTest : MonoBehaviour
{
    public Transform Target;
    public float RotationSpeed = 5f;

    private Quaternion _targetRotation;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
        {
            var direction = Target.position - transform.position;
            direction.y = 0;
            _targetRotation = Quaternion.LookRotation(direction);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, RotationSpeed * Time.deltaTime);
    }
}
