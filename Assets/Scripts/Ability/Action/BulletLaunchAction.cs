using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

            GameManager.ActorManager.OnAddActorEvent += OnAddActorEvent;
            GameManager.ActorManager.AddActor(bullet);
        }

        // todo 生命周期只有1帧，回调回来时已经销毁
        private void OnAddActorEvent(ActorModel actor)
        {
            if (actor.ActorData.Id == bullet)
            {
                actor.Owner = tree.ActorModel;
            }
        }

        protected override void OnExit()
        {
            GameManager.ActorManager.OnAddActorEvent -= OnAddActorEvent;
            base.OnExit();
        }
    }
}

