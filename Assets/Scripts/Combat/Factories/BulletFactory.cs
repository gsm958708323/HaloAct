using System;
using UnityEngine;

namespace Combat
{
    public class BulletFactory
    {
        public World world;
        ConfigManager configManager;

        public BulletFactory(World world, ConfigManager configManager)
        {
            this.world = world;
            this.configManager = configManager;
        }

        public Entity Create(int configId, Entity source,
        Vector3 spawnPos, Vector3 direction,
        Entity trackingTarget = default)
        {
            BulletConfig cfg = configManager.GetBulletConfig(configId);
            var entity = world.CreateEntity();

            var info = world.AddComponent<SourceInfoComponent>(entity);
            info.ConfigId = cfg.BulletId;
            info.Type = EntityType.Bullet;
            info.Source = source;

            var sourceFaction = world.GetComponent<FactionComponent>(source);
            if (sourceFaction != null)
            {
                var faction = world.AddComponent<FactionComponent>(entity);
                faction.FactionId = sourceFaction.FactionId;
            }

            var transfrom = world.AddComponent<TransformComponent>(entity);
            transfrom.Position = spawnPos;
            transfrom.Forward = direction.normalized;

            var collider = world.AddComponent<ColliderComponent>(entity);
            collider.Data = ColliderData.CreateSphere(spawnPos, cfg.CollisionRadius);

            var traj = world.AddComponent<TrajectoryComponent>(entity);
            traj.Type = cfg.Trajectory;
            traj.Speed = cfg.Speed;
            traj.Origin = spawnPos;
            traj.PreviousPosition = spawnPos;
            PopulateTrajectoryParams(traj, cfg, spawnPos, direction, trackingTarget);

            var lefetime = world.AddComponent<LifetimeComponent>(entity);
            lefetime.Duration = cfg.LifeTime;
            lefetime.Remaining = cfg.LifeTime;

            var hitCfg = world.AddComponent<HitConfigComponent>(entity);
            hitCfg.Mode = cfg.HitMode;
            hitCfg.MaxHitCount = cfg.MaxHitCount;
            hitCfg.BounceRange = cfg.BounceRange;

            var payload = world.AddComponent<EffectPayloadComponent>(entity);
            payload.Groups = cfg.PayloadGroups;
            if (cfg.HitMode != HitMode.Single)
            {
                world.AddComponent<TargetMemoryComponent>(entity);
            }
            if (cfg.FollowSource)
            {
                var follow = world.AddComponent<FollowComponent>(entity);
                follow.Target = source;
                follow.DestroyOnTargetDead = true;
            }

            return entity;
        }

        private void PopulateTrajectoryParams(TrajectoryComponent traj, BulletConfig cfg, Vector3 spawnPos, Vector3 direction, Entity trackingTarget)
        {
            switch (cfg.Trajectory)
            {
                case TrajectoryType.Straight:
                    traj.Forward = direction;
                    break;
                case TrajectoryType.Parabola:
                    traj.ParamFloat0 = cfg.TrajectoryParams[0];// gravity
                    // 计算初始速度：水平方向+仰角
                    float angle = cfg.TrajectoryParams[1]; // 发射羊角
                    traj.ParamVec0 = Quaternion.AngleAxis(-angle, Vector3.Cross(direction, Vector3.up)) * direction * cfg.Speed;
                    break;
            }
        }
    }
}
