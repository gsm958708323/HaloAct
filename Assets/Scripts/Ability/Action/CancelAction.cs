using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class CancelAction : AbilityAction
    {
        public bool CanCancel;

        override public void Tick(AbilityBehaviorTree t)
        {
            base.Tick(t);

            t.CanCancel = CanCancel;
            Debug.Log("CancelAction Tick");
        }
    }
}

