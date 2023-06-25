using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;

public class TestAction : AbilityAction
{
    override public void OnEnter(ActorModel t)
    {
        base.OnEnter(t);
        Debug.Log("OnEnter");
    }
}
