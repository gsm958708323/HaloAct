using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    Vector3 checkGroundPos;

    GroundCheckData checkerData;

    TransfromComp transfromComp;

    public void Init(ActorModel actorModel)
    {
        this.checkerData = actorModel.GetComp<ActorDataComp>().Data.CheckerData;
        transfromComp = actorModel.GetComp<TransfromComp>();
    }

    public bool CheckGround()
    {
        checkGroundPos.Set(transfromComp.Position.x, transfromComp.Position.y + checkerData.GroundSetting.y, transfromComp.Position.z);
        return Physics.CheckSphere(checkGroundPos, checkerData.GroundSetting.x, checkerData.GroundLayer, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmos()
    {
        checkGroundPos.Set(transfromComp.Position.x, transfromComp.Position.y + checkerData.GroundSetting.y, transfromComp.Position.z);
        Gizmos.DrawWireSphere(checkGroundPos, checkerData.GroundSetting.x);
    }
}
