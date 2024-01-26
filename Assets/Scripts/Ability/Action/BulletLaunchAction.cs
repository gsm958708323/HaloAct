using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class BulletLaunchAction : AbilityAction
    {
        public BulletBehavior bullet;

        protected override void OnTick(int curFrame)
        {
            base.OnTick(curFrame);

        }
    }
}

