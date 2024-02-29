using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ability
{
    public class CreateBulletAction : AbilitySimpleAction
    {
        public int bullet;

        protected override void OnEnter()
        {
            var actor = GameManager.ActorManager.AddActor(bullet);
            if (actor)
                actor.Creater = tree.ActorModel;
        }
    }
}

