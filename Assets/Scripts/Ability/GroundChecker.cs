using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    Vector3 checkGroundPos;
    /// <summary>
    /// x为检测半径，y为偏移量
    /// </summary>
    public Vector2 CheckGroundSetting = new(0.5f, 0.75f);
    public LayerMask groundLayer;

    public bool CheckGround()
    {
        checkGroundPos.Set(transform.position.x, transform.position.y + CheckGroundSetting.y, transform.position.z);
        return Physics.CheckSphere(checkGroundPos, CheckGroundSetting.x, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmos()
    {
        checkGroundPos.Set(transform.position.x, transform.position.y + CheckGroundSetting.y, transform.position.z);
        Gizmos.DrawWireSphere(checkGroundPos, CheckGroundSetting.x);
    }
}
