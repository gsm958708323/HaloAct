using System;
using System.Collections;
using System.Collections.Generic;

namespace Ability
{
    public class BuffRemovedAction
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

