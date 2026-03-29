using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 技能打断窗口
    /// </summary>
    public class CancelCondition : AbilityCondition
    {
        override public bool Check(BehaviorComp tree)
        {
            return tree.curNode.CanCancel;
        }
    }
}
