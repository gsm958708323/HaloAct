using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    Action<PlayerAbilityController> triggerCB;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddTriggerCB(Action<PlayerAbilityController> action)
    {
        triggerCB = action;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            triggerCB?.Invoke(other.GetComponent<PlayerAbilityController>());
        }
    }
}
