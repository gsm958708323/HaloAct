using System;
using System.Collections;
using System.Collections.Generic;

namespace Ability
{
    public class BuffOccurAction
    {
        protected BuffModel buff;

        public void Execute(BuffModel buff)
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

