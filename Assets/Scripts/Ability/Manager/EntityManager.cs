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
            var data = FightManager.Config.LoadBullet(id);
            if (data is null)
            {
                return null;
            }

            var bullet = AddEntity(EntityType.Bullet);
            var dataComp = bullet.AddComp<BulletDataComp>();
            dataComp.Data = data;
            dataComp.Speed = data.Speed;
            dataComp.Duration = data.Duration;
            dataComp.TimeElapsed = 0;
            dataComp.Hp = data.HitTimes;
            dataComp.Caster = launcher.Caster;
            dataComp.FirDegree = launcher.FireDegree;

            var trans = bullet.AddComp<SimpleTransformComp>();
            trans.Position = launcher.Position;
            trans.Rotation = launcher.Rotation;

            GameManager.Dispatcher.Notify<Entity>(EventId.CreateEntity, bullet);
            return bullet;
        }
    }
}
