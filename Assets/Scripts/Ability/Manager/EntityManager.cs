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
            entity.AddComp<AttrComp>();
            var transfromComp = entity.AddComp<TransfromComp>();
            transfromComp.SetBornPos(data.BornPosInfo);

            var velocityComp = entity.AddComp<VelocityComp>();
            velocityComp.DelayAerialTime = data.DelayAerialTime;
            velocityComp.Gravity = data.Gravity;
            velocityComp.Frictional = data.Frictional;

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
            dataComp.Speed = launcher.SpeedOverride ?? data.Speed;
            dataComp.Duration = launcher.DurationOverride ?? data.Duration;
            dataComp.TimeElapsed = 0;
            dataComp.CanHitAfterCreated = launcher.CanHitAfterCreatedOverride ?? data.CanHitAfterCreated;
            dataComp.RemoveReason = BulletRemoveReason.None;
            dataComp.Hp = data.HitTimes;

            data.OnCreate?.Execute(dataComp, null);
            GameManager.Dispatcher.Notify<Entity>(EventId.CreateEntity, actor);
            return actor;
        }

        public Entity CreateAoe(AoeLauncher launcher)
        {
            var id = launcher.AoeId;
            var data = launcher.Data ?? FightManager.Config.LoadAoe(id);
            if (data is null)
            {
                return null;
            }

            var aoe = AddEntity(EntityType.Aoe);
            var dataComp = aoe.AddComp<AoeDataComp>();
            dataComp.Data = data;
            dataComp.Caster = launcher.Caster;
            dataComp.Position = launcher.Position;
            dataComp.Velocity = launcher.Velocity;
            dataComp.Radius = data.Radius;
            dataComp.Duration = data.Duration;
            dataComp.TickInterval = data.TickInterval;
            dataComp.TimeElapsed = 0f;

            data.OnCreate?.Execute(dataComp, null);
            GameManager.Dispatcher.Notify<Entity>(EventId.CreateEntity, aoe);
            return aoe;
        }

        protected override void OnEntityRemoved(int uid, IEntity entity)
        {
            base.OnEntityRemoved(uid, entity);
            GameManager.Dispatcher.Notify<int>(EventId.RemoveEntity, uid);
        }
    }
}
