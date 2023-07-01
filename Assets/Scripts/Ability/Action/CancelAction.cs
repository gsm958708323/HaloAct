using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 打断当前行为
    /// </summary>
    public class CancelAction : AbilityAction
    {
        public bool CanCancel;

        override public void Tick(AbilityBehaviorTree t)
        {
            base.Tick(t);

            t.CanCancel = CanCancel;
        }
    }
}

