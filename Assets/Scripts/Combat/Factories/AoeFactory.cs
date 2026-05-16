using UnityEngine;

namespace Combat
{
    public class AoeFactory
    {
        private readonly World world;
        private readonly ConfigManager configManager;

        public AoeFactory(World world, ConfigManager configManager)
        {
            this.world = world;
            this.configManager = configManager;
        }

        public Entity Create(int configId, Entity source, Entity instigator,
        Vector3 position, Vector3 forward)
        {
            var cfg = configManager.GetAOEConfig(configId);
            if (cfg == null) return Entity.Null;

            var entity = world.CreateEntity();

            var info = world.AddComponent<SourceInfoComponent>(entity);
            info.ConfigId = cfg.AoeId;
            info.Type = EntityType.AOE;
            info.Source = source;
            info.Instigator = instigator;

            var sourceFaction = world.GetComponent<FactionComponent>(source);
            if (sourceFaction != null)
            {
                var faction = world.AddComponent<FactionComponent>(entity);
                faction.FactionId = sourceFaction.FactionId;
            }

            var transform = world.AddComponent<TransformComponent>(entity);
            transform.Position = position;
            transform.Forward = forward.normalized;

            var collider = world.AddComponent<ColliderComponent>(entity);
            collider.Data = BuildColliderData(cfg, position, forward);

            var tick = world.AddComponent<TickTimerComponent>(entity);
            tick.Interval = cfg.TickInterval;
            tick.Timer = 0;

            var payload = world.AddComponent<EffectPayloadComponent>(entity);
            payload.Groups = cfg.PayloadGroups;

            world.AddComponent<TargetMemoryComponent>(entity);

            if (cfg.FollowSource)
            {
                var follow = world.AddComponent<FollowComponent>(entity);
                follow.Target = source;
                follow.InheritRotation = true;
                follow.DestroyOnTargetDead = false;
            }
            return entity;
        }

        private ColliderData BuildColliderData(
    AoeConfig cfg, Vector3 pos, Vector3 forward)
        {
            switch (cfg.Shape)
            {
                case ShapeType.Circle:
                case ShapeType.Sector:
                    return ColliderData.CreateSphere(pos, cfg.ShapeParams[0]);

                case ShapeType.Rectangle:
                    float halfWidth = cfg.ShapeParams[0];
                    float halfDepth = cfg.ShapeParams[1];
                    float halfHeight = cfg.ShapeParams.Length > 2
                        ? cfg.ShapeParams[2] : 1f;
                    Vector3 center = pos + forward * halfDepth;
                    return ColliderData.CreateBox(center,
                        new Vector3(halfWidth, halfHeight, halfDepth),
                        Quaternion.LookRotation(forward));

                default:
                    return ColliderData.CreateSphere(pos, 1f);
            }
        }
    }
}
