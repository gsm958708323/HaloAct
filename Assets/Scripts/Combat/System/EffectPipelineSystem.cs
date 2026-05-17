using System.Collections.Generic;

namespace Combat
{
    public class EffectPipelineSystem : ISystem
    {
        private const int MaxIterations = 256;
        private readonly List<IEffectProcessor> processors;
        private readonly EffectRequestBuffer buffer;

        public EffectPipelineSystem(
            EffectRequestBuffer buffer,
            IEventBus eventBus,
            BuffFactory buffFactory,
            BulletFactory bulletFactory,
            AoeFactory aoeFactory)
        {
            this.buffer = buffer;

            processors = new List<IEffectProcessor>
            {
                new ImmunityProcessor(),
                new ModifierProcessor(),
                new ShieldProcessor(eventBus),
                new ApplyProcessor(eventBus, buffFactory, bulletFactory, aoeFactory),
                // new ReactionProcessor(buffer),
            };
        }

        public override void Tick(float delteTime)
        {
            var requests = buffer.GetCurrent();
            int processed = 0;
            // 防止意外无限遍历
            for (int i = 0; i < requests.Count && processed < MaxIterations; i++)
            {
                processed++;
                var request = requests[i];

                if (!world.IsAlive(request.Target))
                {
                    // 目标已死亡，SpawnAOE/SpawnBullet 仍可执行
                    if (request.Type != EffectType.SpawnAOE && request.Type != EffectType.SpawnBullet)
                    {
                        continue;
                    }
                }

                for (int p = 0; p < processors.Count; p++)
                {
                    if (request.Cancelled) break;
                    processors[p].Process(request, world);
                }
            }

            buffer.Flush();
        }
    }
}