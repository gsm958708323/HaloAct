using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    Vector3 checkGroundPos;

    GroundCheckData checkerData;

    public void Init(GroundCheckData data)
    {
        this.checkerData = data;
    }

    public bool CheckGround()
    {
        checkGroundPos.Set(transform.position.x, transform.position.y + checkerData.GroundSetting.y, transform.position.z);
        return Physics.CheckSphere(checkGroundPos, checkerData.GroundSetting.x, checkerData.GroundLayer, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmos()
    {
        checkGroundPos.Set(transform.position.x, transform.position.y + checkerData.GroundSetting.y, transform.position.z);
        Gizmos.DrawWireSphere(checkGroundPos, checkerData.GroundSetting.x);
    }
}
