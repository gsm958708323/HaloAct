using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class BulletLaunchAction : AbilityAction
    {
        public int bullet;

        // Yaw offset (degrees) relative to caster forward.
        public float fireDegree;

        protected override void OnEnter()
        {
            base.OnEnter();
            if (StartFrame != EndFrame)
            {
                Debugger.LogError($"连续多帧创建子弹 ", LogDomain.Bullet);
                EndFrame = StartFrame;
            }

            if (bullet <= 0)
            {
                Debugger.LogError($"无效子弹Id {bullet}", LogDomain.Bullet);
                return;
            }

            var caster = tree?.Entity;
            if (caster is null)
                return;

            var casterTrans = caster.GetComp<TransfromComp>();
            if (casterTrans is null)
            {
                Debugger.LogError($"创建子弹失败：缺少TransfromComp {caster.Uid}", LogDomain.Bullet);
                return;
            }

            var bulletData = FightManager.Config.LoadBullet(bullet);
            var offset = bulletData != null ? bulletData.SpawnOffset : Vector3.zero;
            var spawnPos = casterTrans.Position + casterTrans.right * offset.x + casterTrans.up * offset.y + casterTrans.forward * offset.z;

            var launcher = new BulletLauncher
            {
                BulletId = bullet,
                Caster = caster,
                FireDegree = fireDegree,
                Position = spawnPos,
                Rotation = casterTrans.Rotation * Quaternion.Euler(0, fireDegree, 0)
            };

            FightManager.LogicEntity.CreateBullet(launcher);

        }
    }
}

