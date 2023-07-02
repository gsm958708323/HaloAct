using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

namespace Ability
{
    public class CancelCondition : AbilityCondition
    {
        override public bool Check(AbilityBehaviorTree tree)
        {
            return tree.CanCancel;
        }
    }
}
