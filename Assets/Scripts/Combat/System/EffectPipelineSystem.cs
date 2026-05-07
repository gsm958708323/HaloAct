using System.Collections.Generic;
using UnityEditor.UIElements;

namespace Combat
{
    public class EffectPipelineSystem : ISystem
    {
        List<IEffectProcessor> processors;
        List<EffectRequest> currentFrameQueue;
        List<EffectRequest> nextFrameQueue;
        World world;

        public EffectPipelineSystem()
        {
            processors = new()
            {
                new ImmunityProcessor(),
                new ModifierProcessor(),
                new ShieldProcessor(),
                new ApplyProcessor(),
                new ReactionProcessor(),
            };
            currentFrameQueue = new List<EffectRequest>();
            nextFrameQueue = new List<EffectRequest>();
        }

        public void Init(World world)
        {
            this.world = world;
        }

        public void Tick(float delteTime)
        {
            foreach (var request in currentFrameQueue)
            {
                foreach (var item in processors)
                {
                    if (request.Cancelled) break;
                    item.Process(request, world);
                }
            }
            currentFrameQueue.Clear();
        }

        public void Submit(EffectRequest request)
        {
            currentFrameQueue.Add(request);
        }
    }
}