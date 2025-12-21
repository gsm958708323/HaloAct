using System;
using System.Collections;
using System.Collections.Generic;

namespace Ability
{
    public class BuffCastAction
    {
        private EffectObj buff;
        private AbilityNode abilityNode;

        public AbilityNode Execute(EffectObj buff, AbilityNode node)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.Buff);
            this.buff = buff;
            this.abilityNode = node;
            return OnExecute();
        }

        protected virtual AbilityNode OnExecute()
        {
            return null;
        }
    }
}

