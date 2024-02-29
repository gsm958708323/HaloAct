using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ability
{
    public class AddBuffAction : AbilitySimpleAction
    {
        public int BuffId;

        public int Target;

        protected override void OnEnter()
        {
            var actor = GameManager.ActorManager.GetActor(Target);
            if (actor)
            {
                actor.Buff.AddBuff(BuffId);
            }
        }
    }
}

