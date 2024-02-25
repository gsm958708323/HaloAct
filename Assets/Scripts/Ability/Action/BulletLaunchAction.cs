using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class BulletLaunchAction : AbilityAction
    {
        public BulletBehavior bullet;

        protected override void OnEnter()
        {
            base.OnEnter();
            if (StartFrame != EndFrame)
            {
                Debugger.LogError($"连续多帧创建子弹 ", LogDomain.Bullet);
                EndFrame = StartFrame;
            }
        }

        protected override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);

        }
    }
}

