using System;
using System.Collections;
using System.Collections.Generic;

namespace Ability
{
    public class AddBuffAction : AbilitySimpleAction
    {
        public AddBuffInfo buffInfo;

        protected override void OnExecute()
        {
            var actor = GameManager.Actor.GetActor(buffInfo.Target);
            if (actor)
            {
                actor.Buff.AddBuff(buffInfo);
            }
        }
    }
}

