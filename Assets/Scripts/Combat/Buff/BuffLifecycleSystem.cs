
using System;

namespace Combat
{
    public class BuffLifecycleSystem : ISystem
    {
        private readonly EffectRequestBuffer effectBuffer;
        private readonly IEventBus eventBus;

        public BuffLifecycleSystem(EffectRequestBuffer effectBuffer, IEventBus eventBus)
        {
            this.effectBuffer = effectBuffer;
            this.eventBus = eventBus;
        }

        public override void Tick(float delteTime)
        {
            foreach (var owner in world.Query<BuffContainerComponent>())
            {
                var container = world.GetComponent<BuffContainerComponent>(owner);

                for (int i = container.Instances.Count - 1; i >= 0; i--)
                {
                    BuffInstance buff = container.Instances[i];
                    if (buff.Duration > 0)
                    {
                        buff.Remaining -= delteTime;
                        if (buff.Remaining <= 0)
                        {
                            RemoveBuff(owner, container, i,
                                         BuffRemovalReason.Expired);
                            continue;
                        }
                    }

                    if (buff.DependsOnBuffId > 0)
                    {
                        if (!HasBuff(container, buff.DependsOnBuffId))
                        {
                            RemoveBuff(owner, container, i,
                                BuffRemovalReason.DependencyLost);
                        }
                    }
                }
            }
        }

        private void RemoveBuff(Entity owner, BuffContainerComponent container,
            int index, BuffRemovalReason reason)
        {
            var buff = container.Instances[index];
            // 1. 触发 OnRemove 效果（死亡时跳过）
            if (reason != BuffRemovalReason.OwnerDeath)
            {
                var removeEffects = PayloadHelper.GetEffects(
                    buff.PayloadGroups, PayloadTrigger.OnRemove);
                EffectSubmitHelper.Submit(effectBuffer,
                    buff.Source, owner, owner, removeEffects);
            }
            container.Instances.RemoveAt(index);

            var modCache = world.GetComponent<ModifierCacheComponent>(owner);
            if (modCache != null) modCache.Dirty = true;

            eventBus.Publish(new BuffRemovedEvent
            {
                Owner = owner,
                ConfigId = buff.ConfigId,
                Reason = reason,
            });

            // 检查是否有其他buff依赖此buff
            for (int j = container.Instances.Count - 1; j >= 0; j--)
            {
                var other = container.Instances[j];
                if (other.DependsOnBuffId == buff.ConfigId)
                {
                    RemoveBuff(owner, container, j, BuffRemovalReason.DependencyLost);
                }
            }
        }

        private bool HasBuff(BuffContainerComponent container, int configId)
        {
            for (int i = 0; i < container.Instances.Count; i++)
            {
                if (container.Instances[i].ConfigId == configId)
                    return true;
            }
            return false;
        }

    }

    public enum BuffRemovalReason
    {
        Expired,
        DependencyLost,
        OwnerDeath,
        Replaced,
        Manual
    }
    // 事件定义
    public struct BuffRemovedEvent
    {
        public Entity Owner;
        public int ConfigId;
        public BuffRemovalReason Reason;
    }

    public struct BuffAppliedEvent
    {
        public Entity Owner;
        public int ConfigId;
        public int CurrentStacks;
    }
}