using System;

namespace Combat
{
    public class ImmunityProcessor : IEffectProcessor
    {
        public string Name => "免疫buff处理";

        public void Process(EffectRequest request, World world)
        {
            if (request.Target.IsNull) return;
            if (!world.IsAlive(request.Target)) return;

            var container = world.GetComponent<BuffContainerComponent>(request.Target);
            if (container == null) return;

            for (int i = 0; i < container.Instances.Count; i++)
            {
                var buff = container.Instances[i];
                if (buff.GrantsImmunityTo == null) continue;

                for (int g = 0; g < container.Instances.Count; g++)
                {
                    var tag = buff.GrantsImmunityTo[g];
                    // 对伤害和增益免疫检查
                    if (request.Type == EffectType.Damage || request.Type == EffectType.Heal)
                    {
                        if (DamageTypeMatchesGroup(request.DamageType, tag))
                        {
                            request.Cancelled = true;
                            return;
                        }
                    }

                }
            }
        }

        private bool DamageTypeMatchesGroup(DamageType damageType, BuffGroupTag tag)
        {
            return tag switch
            {
                BuffGroupTag.Burn => damageType == DamageType.Fire,
                BuffGroupTag.Freeze => damageType == DamageType.Ice,
                BuffGroupTag.Poison => damageType == DamageType.Poison,
                _ => false,
            };
        }
    }
}