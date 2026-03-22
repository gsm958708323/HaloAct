using System;
using System.Collections.Generic;

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

            var entity = AddEntity(EntityType.Actor);
            var dataComp = entity.AddComp<PlayerDataComp>();
            dataComp.Data = data;
            entity.AddComp<TransfromComp>();
            entity.AddComp<BehaviorComp>();
            entity.AddComp<EffectComp>();
            entity.AddComp<AttackComp>();

            GameManager.Dispatcher.Notify<Entity>(EventId.CreateEntity, entity);
            return entity;
        }

        public Entity CreateBullet(BulletLauncher launcher)
        {
            var id = launcher.BulletId;
            var data = launcher.Data ?? FightManager.Config.LoadBullet(id);
            if (data is null)
            {
                return null;
            }

            var actor = AddEntity(EntityType.Bullet);
            var dataComp = actor.AddComp<BulletDataComp>();
            dataComp.Data = data;
            dataComp.Caster = launcher.Caster;
            dataComp.Position = launcher.Position;
            dataComp.Direction = launcher.Direction == UnityEngine.Vector3.zero ? UnityEngine.Vector3.forward : launcher.Direction.normalized;
            dataComp.Speed = data.Speed;
            dataComp.Duration = data.Duration;
            dataComp.TimeElapsed = 0;
            dataComp.Hp = data.HitTimes;

            data.OnCreate?.Execute(dataComp, null);
            GameManager.Dispatcher.Notify<Entity>(EventId.CreateEntity, actor);
            return actor;
        }

        protected override void OnEntityRemoved(int uid, IEntity entity)
        {
            base.OnEntityRemoved(uid, entity);
            GameManager.Dispatcher.Notify<int>(EventId.RemoveEntity, uid);
        }
    }
}
