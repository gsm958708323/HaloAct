using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    Vector3 checkGroundPos;

    GroundCheckData checkerData;
    ActorModel actorModel;

    public void Init(ActorModel actorModel)
    {
        this.actorModel = actorModel;
        this.checkerData = actorModel.ActorData.CheckerData;
    }

    public bool CheckGround()
    {
        checkGroundPos.Set(actorModel.Position.x, actorModel.Position.y + checkerData.GroundSetting.y, actorModel.Position.z);
        return Physics.CheckSphere(checkGroundPos, checkerData.GroundSetting.x, checkerData.GroundLayer, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmos()
    {
        checkGroundPos.Set(actorModel.Position.x, actorModel.Position.y + checkerData.GroundSetting.y, actorModel.Position.z);
        Gizmos.DrawWireSphere(checkGroundPos, checkerData.GroundSetting.x);
    }
}
