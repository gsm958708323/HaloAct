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
        override public void Tick(int curFrame)
        {
            base.Tick(curFrame);

            tree.curNode.CanCancel = true;
        }
    }
}

