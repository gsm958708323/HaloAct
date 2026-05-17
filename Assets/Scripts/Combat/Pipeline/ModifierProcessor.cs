using System;

namespace Combat
{
    public class ModifierProcessor : IEffectProcessor
    {
        public string Name => "属性处理";

        public void Process(EffectRequest request, World world)
        {
            if (request.Type != EffectType.Damage && request.Type != EffectType.Heal)
                return;

            if (!request.Source.IsNull && world.IsAlive(request.Source))
            {
                var sourceCache = world.GetComponent<ModifierCacheComponent>(request.Source);
                if (sourceCache != null)
                {
                    request.Value = ApplyModifiers(request.Value, sourceCache,
                     ModifierTarget.Outgoing, request.DamageType);
                }
            }

            if (world.IsAlive(request.Target))
            {
                var targetCache = world.GetComponent<ModifierCacheComponent>(request.Target);
                if (targetCache != null)
                {
                    request.Value = ApplyModifiers(request.Value, targetCache, ModifierTarget.Incoming, request.DamageType);
                }
            }

            if (request.Type == EffectType.Damage && request.Value < 0)
                request.Value = 0;
        }

        private float ApplyModifiers(float baseValue,
            ModifierCacheComponent cache,
            ModifierTarget direction,
            DamageType damageType)
        {
            float value = baseValue;

            var specific = cache.Get(direction, damageType);
            if (specific != null)
                value = specific.Apply(value);

            if (damageType != DamageType.None)
            {
                var gengral = cache.Get(direction, DamageType.None);
                if (gengral != null)
                    value = gengral.Apply(value);
            }
            return value;
        }
    }
}