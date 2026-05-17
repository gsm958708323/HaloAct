using System;

namespace Combat
{
    public class BuffTickEffectSystem : ISystem
    {
        private const int MaxTicksPerFrame = 5;
        private readonly EffectRequestBuffer effectBuffer;
        public BuffTickEffectSystem(EffectRequestBuffer effectBuffer)
        {
            this.effectBuffer = effectBuffer;
        }

        public override void Tick(float delteTime)
        {
            foreach (var owner in world.Query<BuffContainerComponent>())
            {
                var container = world.GetComponent<BuffContainerComponent>(owner);

                for (int i = 0; i < container.Instances.Count; i++)
                {
                    var buff = container.Instances[i];
                    if (buff.TickInterval <= 0f) continue;
                    buff.TickTimer -= delteTime;

                }

                foreach (var buff in container.Instances)
                {
                    if (buff.TickInterval <= 0) continue;
                    buff.TickTimer -= delteTime;

                    int ticksThisFrame = 0;
                    while (buff.TickTimer <= 0f && ticksThisFrame < MaxTicksPerFrame)
                    {
                        buff.TickTimer += buff.TickInterval;
                        ticksThisFrame++;

                        var tickEffects = PayloadHelper.GetEffects(
                            buff.PayloadGroups, PayloadTrigger.OnTick);

                        for (int e = 0; e < tickEffects.Length; e++)
                        {
                            var req = new EffectRequest();
                            req.Type = tickEffects[e].Type;
                            req.Source = buff.Source;
                            req.Instigator = owner; // Buff 宿主
                            req.Target = owner;
                            req.Value = tickEffects[e].Value;
                            req.DamageType = tickEffects[e].DamageType;
                            req.ReferenceId = tickEffects[e].ReferenceId;

                            EffectSubmitHelper.ScaleByStacks(req, buff);
                            effectBuffer.Submit(req);
                        }
                    }
                }
            }
        }
    }
}