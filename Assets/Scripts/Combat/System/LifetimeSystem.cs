using System;

namespace Combat
{
    public class LifetimeSystem : ISystem
    {
        private readonly EffectRequestBuffer effectBuffer;

        public LifetimeSystem(EffectRequestBuffer effectBuffer)
        {
            this.effectBuffer = effectBuffer;
        }

        public override void Tick(float delteTime)
        {
            var entities = world.Query<LifetimeComponent>();

            foreach (var entity in entities)
            {
                if (world.HasComponent<DestroyTagComponent>(entity)) continue;

                var lifetime = world.GetComponent<LifetimeComponent>(entity);
                lifetime.Remaining -= delteTime;
                if (lifetime.Remaining > 0) continue;

                // 提交 OnExpire 效果
                SubmitExpireEffects(entity);

                // 标记销毁
                world.AddComponent<DestroyTagComponent>(entity);
            }
        }

        private void SubmitExpireEffects(Entity entity)
        {
            var payload = world.GetComponent<EffectPayloadComponent>(entity);
            if (payload == null) return;

            var effects = PayloadHelper.GetEffects(payload, PayloadTrigger.OnExpire);
            if (effects.Length == 0) return;

            var sourceInfo = world.GetComponent<SourceInfoComponent>(entity);
            if (sourceInfo == null) return;

            var transform = world.GetComponent<TransformComponent>(entity);
            var pos = transform != null ? transform.Position : default;
            var fwd = transform != null ? transform.Forward : default;

            EffectSubmitHelper.Submit(effectBuffer, sourceInfo.Source, entity, Entity.Null, effects, pos, fwd);
        }
    }
}