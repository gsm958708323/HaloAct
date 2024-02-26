using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class BulletLaunchAction : AbilityAction
    {
        public int bullet;

        protected override void OnEnter()
        {
            base.OnEnter();
            if (StartFrame != EndFrame)
            {
                Debugger.LogError($"连续多帧创建子弹 ", LogDomain.Bullet);
                EndFrame = StartFrame;
            }

            GameManager.ActorManager.AddActor(bullet);
        }
    }
}

