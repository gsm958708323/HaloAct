using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;

public class TestAction : AbilityAction
{
    override public void Enter(AbilityBehaviorTree t)
    {
        base.Enter(t);
    }

    override public void Tick(AbilityBehaviorTree t)
    {
        base.Tick(t);
        // Debug.Log("Tick");
    }

    override public void Exit(AbilityBehaviorTree t)
    {
    }
}
