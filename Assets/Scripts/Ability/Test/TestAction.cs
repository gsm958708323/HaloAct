using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;

public class TestAction : AbilityAction
{
    override public void Enter()
    {
        base.Enter();
    }

    override public void Tick()
    {
        base.Tick();
        // Debug.Log("Tick");
    }

    override public void Exit()
    {
    }
}
