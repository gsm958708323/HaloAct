using System;

namespace Combat
{
    public class ShieldProcessor : IEffectProcessor
    {
        private readonly IEventBus eventBus;
        public string Name => "护盾处理";
        public ShieldProcessor(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }


        public void Process(EffectRequest request, World world)
        {
            if (request.Type != EffectType.Damage) return;
            if (!world.IsAlive(request.Target)) return;

            var container = world.GetComponent<BuffContainerComponent>(request.Target);
            if (container == null) return;

            // 按优先级从高到低排序吸收
            // 遍历时可能移除护盾，倒序安全
            for (int i = container.Instances.Count - 1; i >= 0; i--)
            {
                if (request.Value <= 0) break;

                var buff = container.Instances[i];
                if (!buff.IsShield) continue;

                if (buff.ShieldAbsorbType != DamageType.None && buff.ShieldAbsorbType != request.DamageType)
                    continue;

                float absorb = MathF.Min(buff.ShieldValue, request.Value);
                buff.ShieldValue -= absorb;
                request.Value -= absorb;

                // 护盾耗尽
                if (buff.ShieldValue <= 0f)
                {
                    // 标记移除，不再此处移除，延迟到BuffLifecycleSystem
                    buff.Remaining = 0;
                    buff.Duration = 1;

                    eventBus.Publish(new ShieldBreakEvent
                    {
                        Owner = request.Target,
                        BuffConfigId = buff.ConfigId,
                    });
                }
            }

            if (request.Value <= 0)
            {
                request.Value = 0;
                request.Absorbed = true;
            }
        }
    }

    internal struct ShieldBreakEvent
    {
        public Entity Owner { get; set; }
        public int BuffConfigId { get; set; }
    }
}