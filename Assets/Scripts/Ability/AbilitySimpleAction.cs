using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using System;

namespace Ability
{
    /// <summary>
    /// 简单行为，一帧执行
    /// </summary>
    [Serializable]
    public abstract class AbilitySimpleAction
    {
        protected EffectObj buff;

        public void Execute(EffectObj buff)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.Buff);
            this.buff = buff;
            OnExecute();
        }

        protected virtual void OnExecute()
        {
        }
    }
}

