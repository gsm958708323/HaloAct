using UnityEngine;
using System.Collections;
using NodeCanvas.Framework;

public class DataBindExample : MonoBehaviour
{
    [SerializeField]
    private float _myFloat;
    private float timer;

    ///This property is Data Bound to the blackboard float variable
    public float myFloat {
        get { return _myFloat; }
        set
        {
            _myFloat = value;
            Debug.Log("Property value set to: " + value.ToString());
        }
    }

    ///This action is called with the "Implemented Action" action task
    public Status WaitAction(float waitTime) {

        if ( timer >= waitTime ) {
            timer = 0;
            return Status.Success;
        }

        timer += Time.deltaTime;
        return Status.Running;
    }
}