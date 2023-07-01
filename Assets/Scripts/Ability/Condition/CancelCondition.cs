using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

namespace Ability
{
    public class CancelCondition : AbilityCondition
    {
        override public bool Check(AbilityBehaviorTree t)
        {
            return t.CanCancel;
        }
    }
}
