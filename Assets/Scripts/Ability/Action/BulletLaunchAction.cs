using System;
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

            var caster = tree?.Entity;
            if (caster is null)
            {
                return;
            }

            var transComp = caster.GetComp<TransfromComp>();
            if (transComp is null)
            {
                return;
            }

            var bulletData = FightManager.Config.LoadBullet(bullet);
            if (bulletData is null)
            {
                return;
            }

            var launcher = new BulletLauncher
            {
                BulletId = bullet,
                Data = bulletData,
                Caster = caster,
                Position = transComp.Position + transComp.Rotation * bulletData.SpawnOffset,
                Direction = transComp.forward.normalized,
            };

            if (launcher.Direction == Vector3.zero)
            {
                launcher.Direction = Vector3.forward;
            }

            FightManager.LogicEntity.CreateBullet(launcher);

        }
    }
}

