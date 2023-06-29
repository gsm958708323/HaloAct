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

    override public void Tick(ActorModel t)
    {
        base.Tick(t);
        Debug.Log("Tick");
    }

    override public void Exit(ActorModel t)
    {
        Debug.Log("Exit");
        base.Exit(t);
    }
}
