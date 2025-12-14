using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class CreateBulletAction : AbilitySimpleAction
    {
        public int bullet;

        protected override void OnExecute()
        {
            // var actor = GameManager.Actor.AddActor(bullet);
            // if (actor)
            // {
            //     actor.Creater = buff.ActorModel;
            // }
        }
    }
}

