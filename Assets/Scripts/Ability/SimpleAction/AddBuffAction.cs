using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ability
{
    public class AddBuffAction : AbilitySimpleAction
    {
        public AddBuffInfo buffInfo;

        protected override void OnEnter()
        {
            var actor = GameManager.ActorManager.GetActor(buffInfo.Target);
            if (actor)
            {
                buffInfo.Creater = tree.ActorModel;
                actor.Buff.AddBuff(buffInfo);
            }
        }
    }
}

