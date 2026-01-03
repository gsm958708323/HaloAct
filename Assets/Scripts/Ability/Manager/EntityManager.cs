using System;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

namespace Ability
{
    public class EntityManager : IEntityManager
    {
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            // 逻辑帧更新
            DriveEntity(deltaTime);
        }

        public new Entity GetEntity(int uid)
        {
            return base.GetEntity(uid) as Entity;
        }

        public Entity CreateActor(int cfgId)
        {
            var data = FightManager.Config.LoadActor(cfgId);
            if (data is null)
            {
                return null;
            }

            var entity = AddEntity();
            var dataComp = entity.AddComp<PlayerDataComp>();
            dataComp.Data = data;
            entity.AddComp<TransfromComp>();
            entity.AddComp<BehaviorComp>();
            entity.AddComp<EffectComp>();

            GameManager.Dispatcher.Notify<Entity>(EventId.CreateEntity, entity);
            return entity;
        }

        public Entity CreateBullet(BulletLauncher launcher)
        {
            var id = launcher.BulletId;
            var data = FightManager.Config.LoadBullet(id);
            if (data is null)
            {
                return null;
            }

            var actor = AddEntity();
            actor.Enter();
            var dataComp = actor.AddComp<BulletDataComp>();
            dataComp.Data = data;
            return actor;
        }
    }
}
