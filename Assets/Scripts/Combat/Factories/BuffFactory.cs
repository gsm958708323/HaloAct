using System;
using System.Dynamic;
using NodeCanvas.Tasks.Conditions;
using Unity.VisualScripting;
using UnityEngine;

namespace Combat
{
    public class BuffFactory
    {
        private readonly World world;
        private readonly ConfigManager configManager;
        private readonly EffectRequestBuffer effectBuffer;
        private readonly IEventBus eventBus;
        private int instanceIdCounter;

        public BuffFactory(World world, ConfigManager configManager,
            EffectRequestBuffer effectBuffer, IEventBus eventBus)
        {
            this.world = world;
            this.configManager = configManager;
            this.effectBuffer = effectBuffer;
            this.eventBus = eventBus;
        }

        public bool ApplyBuff(Entity owner, int configId, Entity source)
        {
            if (!world.IsAlive(owner)) return false;
            var cfg = configManager.GetBuffConfig(configId);
            if (cfg == null) return false;
            var container = world.GetComponent<BuffContainerComponent>(owner);
            if (container == null) return false;

            // 免疫检查
            if (IsImmnue(container, cfg))
                return false;

            // 互斥组处理
            if (!ResolveGroupConflict(owner, container, cfg))
                return false;

            // 叠层处理
            BuffInstance existing = FindExisting(container, configId, source, cfg);
            if (existing != null)
                return HandleExisting(owner, existing, cfg);

            BuffInstance buff = CreateInstance(owner, source, cfg);
            container.Instances.Add(buff);

            // 藏标记
            MarkDirty(owner);

            SubmitPayload(buff, owner, PayloadTrigger.OnApply);

            eventBus.Publish(new BuffAppliedEvent
            {
                Owner = owner,
                ConfigId = configId,
                CurrentStacks = buff.CurrentStacks,
            });

            return true;
        }

         private BuffInstance CreateInstance(Entity owner, Entity source,
            BuffConfig cfg)
        {
            var buff = new BuffInstance();
            buff.InstanceId = ++instanceIdCounter;
            buff.ConfigId = cfg.BuffId;
            buff.Source = source;
            buff.Owner = owner;

            buff.Duration = cfg.Duration;
            buff.Remaining = cfg.Duration;

            buff.CurrentStacks = 1;
            buff.MaxStacks = cfg.MaxStack > 0 ? cfg.MaxStack : 1;
            buff.StackMode = cfg.StackMode;
            buff.RefreshPolicy = cfg.RefreshPolicy;
            buff.StackThreshold = cfg.StackThreshold;
            buff.ThresholdAction = cfg.ThresholdAction;

            // 复制 Modifiers
            if (cfg.Modifiers != null)
            {
                for (int i = 0; i < cfg.Modifiers.Length; i++)
                    buff.Modifiers.Add(cfg.Modifiers[i]);
            }

            buff.TickInterval = cfg.TickInterval;
            buff.TickTimer = cfg.TickInterval; // 首次 Tick 在 interval 后

            buff.PayloadGroups = cfg.PayloadGroups;

            // 复制 TriggerRules
            if (cfg.TriggerRules != null)
            {
                for (int i = 0; i < cfg.TriggerRules.Length; i++)
                    buff.TriggerRules.Add(cfg.TriggerRules[i]);
            }

            buff.GroupTag = cfg.GroupTag;
            buff.Priority = cfg.Priority;
            buff.GrantsImmunityTo = cfg.GrantsImmunityTo;
            buff.DependsOnBuffId = cfg.DependsOnBuffId;

            buff.IsShield = cfg.IsShield;
            buff.ShieldValue = cfg.ShieldValue;
            buff.ShieldAbsorbType = cfg.ShieldAbsorbType;

            return buff;
        }

        private void SubmitPayload(BuffInstance buff, Entity owner, PayloadTrigger trigger)
        {
            var effects = PayloadHelper.GetEffects(
                buff.PayloadGroups, trigger);
            EffectSubmitHelper.Submit(effectBuffer,
                buff.Source, owner, owner, effects);
        }

        private void MarkDirty(Entity owner)
        {
            var cache = world.GetComponent<ModifierCacheComponent>(owner);
            if (cache != null) cache.Dirty = true;
        }

        private bool HandleExisting(Entity owner, BuffInstance existing, BuffConfig cfg)
        {
            // 刷新持续时间
            RefreshDuration(existing, cfg);
            // 叠层
            if (cfg.MaxStack > 1 && existing.CurrentStacks < existing.MaxStacks)
            {
                existing.CurrentStacks++;
                RebuildModifiers(existing, cfg);
                MarkDirty(owner);
                CheckStackThreshold(owner, existing, cfg);
            }

            eventBus.Publish(new BuffAppliedEvent
            {
                Owner = owner,
                ConfigId = cfg.BuffId,
                CurrentStacks = existing.CurrentStacks,
            });

            return true;
        }

        private void CheckStackThreshold(Entity owner, BuffInstance buff, BuffConfig cfg)
        {
            if (cfg.StackThreshold <= 0) return;
            if (buff.CurrentStacks < cfg.StackThreshold) return;

            // 触发预知效果
            var effects = PayloadHelper.GetEffects(buff.PayloadGroups, PayloadTrigger.OnThreshold);
            EffectSubmitHelper.Submit(effectBuffer, buff.Source, owner, owner, effects);

            // 阈值后行为
            switch (cfg.ThresholdAction)
            {
                case ThresholdAction.ResetStacks:
                    buff.CurrentStacks = 0;
                    RebuildModifiers(buff, cfg);
                    MarkDirty(owner);
                    break;

                case ThresholdAction.RemoveBuff:
                    var container = world.GetComponent<BuffContainerComponent>(owner);
                    int idx = container.Instances.IndexOf(buff);
                    if (idx >= 0)
                        RemoveAtIndex(owner, container, idx, BuffRemovalReason.Manual);
                    break;
                case ThresholdAction.Keep:
                    break;
            }
        }

        private void RebuildModifiers(BuffInstance buff, BuffConfig cfg)
        {
            buff.Modifiers.Clear();
            if (cfg.Modifiers == null) return;
            for (int i = 0; i < cfg.Modifiers.Length; i++)
            {
                var mod = cfg.Modifiers[i];

                if (buff.CurrentStacks > 0 &&
                (mod.Op == ModifierOp.PercentAdd || mod.Op == ModifierOp.FlatAdd))
                {
                    mod = new Modifier(mod.Direction, mod.Op, mod.AffectedType,
                    cfg.Modifiers[i].Value * buff.CurrentStacks);
                }
                buff.Modifiers.Add(mod);
            }
        }

        private void RefreshDuration(BuffInstance buff, BuffConfig cfg)
        {
            if (cfg.Duration <= 0) return;
            switch (cfg.RefreshPolicy)
            {
                case RefreshPolicy.Reset:
                    buff.Remaining = cfg.Duration;
                    break;
                case RefreshPolicy.Max:
                    buff.Remaining = Mathf.Max(buff.Remaining, cfg.Duration);
                    break;
                case RefreshPolicy.Extend:
                    buff.Remaining += cfg.Duration;
                    break;
            }
        }

        private BuffInstance FindExisting(BuffContainerComponent container, int configId, Entity source, BuffConfig cfg)
        {
            for (int i = 0; i < container.Instances.Count; i++)
            {
                var buff = container.Instances[i];
                if (buff.ConfigId != configId) continue;

                switch (cfg.StackMode)
                {
                    case StackMode.None:
                    case StackMode.Shared:
                        return buff;
                    case StackMode.BySource:
                        if (buff.Source == source) return buff;
                        break;
                    case StackMode.Independent:
                        return null; // 总是创建新实例
                }
            }
            return null;
        }

        private bool ResolveGroupConflict(Entity owner, BuffContainerComponent container, BuffConfig cfg)
        {
            if (cfg.GroupTag == BuffGroupTag.None) return true;
            for (int i = container.Instances.Count - 1; i >= 0; i--)
            {
                var other = container.Instances[i];
                // 只处理不同标签的互斥，排除自己
                if (other.GroupTag != cfg.GroupTag) continue;
                if (other.ConfigId == cfg.BuffId) continue;

                if (other.Priority > cfg.Priority)
                    return false; // 现有更强，拒绝

                RemoveAtIndex(owner, container, i, BuffRemovalReason.Replaced);
            }
            return true;
        }

        public void RemoveBuffByConfigId(Entity owner, int configId,
                   BuffRemovalReason reason)
        {
            var container = world.GetComponent<BuffContainerComponent>(owner);
            if (container == null) return;

            for (int i = container.Instances.Count - 1; i >= 0; i--)
            {
                if (container.Instances[i].ConfigId == configId)
                {
                    RemoveAtIndex(owner, container, i, reason);
                    return;
                }
            }
        }

        private void RemoveAtIndex(Entity owner, BuffContainerComponent container, int index, BuffRemovalReason reason)
        {
            var buff = container.Instances[index];
            if (reason != BuffRemovalReason.OwnerDeath)
            {
                SubmitPayload(buff, owner, PayloadTrigger.OnRemove);
            }

            container.Instances.RemoveAt(index);

            MarkDirty(owner);

            eventBus.Publish(new BuffRemovedEvent
            {
                Owner = owner,
                ConfigId = buff.ConfigId,
                Reason = reason,
            });

            // 5. 级联：依赖此 Buff 的其他 Buff
            for (int j = container.Instances.Count - 1; j >= 0; j--)
            {
                if (container.Instances[j].DependsOnBuffId == buff.ConfigId)
                {
                    RemoveAtIndex(owner, container, j,
                        BuffRemovalReason.DependencyLost);
                }
            }
        }

        private bool IsImmnue(BuffContainerComponent container, BuffConfig cfg)
        {
            if (cfg.GroupTag == BuffGroupTag.None) return false;
            for (int i = 0; i < container.Instances.Count; i++)
            {
                var buff = container.Instances[i];
                if (buff.GrantsImmunityTo == null) continue;

                for (int g = 0; g < buff.GrantsImmunityTo.Length; g++)
                {
                    if (buff.GrantsImmunityTo[g] == cfg.GroupTag)
                        return true;
                }
            }
            return false;
        }
    }
}