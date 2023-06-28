using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;

public class TestAction : AbilityAction
{
    override public void Enter(ActorModel t)
    {
        base.Enter(t);
        Debug.Log("OnEnter");
    }
}
