using System;
using UnityEngine;

namespace Combat
{
    public class EffectPayloadComponent : IComponent
    {
        public EffectGroup[] Groups;
    }

    public static class PayloadHelper
    {
        private static readonly EffectConfig[] Empty = System.Array.Empty<EffectConfig>();

        public static EffectConfig[] GetEffects(
            EffectPayloadComponent payload, PayloadTrigger trigger)
        {
            if (payload?.Groups == null) return Empty;
            return GetEffects(payload.Groups, trigger);
        }

        public static EffectConfig[] GetEffects(
            EffectGroup[] groups, PayloadTrigger trigger)
        {
            if (groups == null) return Empty;

            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i].Trigger == trigger)
                    return groups[i].Effects ?? Empty;
            }
            return Empty;
        }
    }

    public static class EffectSubmitHelper
    {
        /// <summary>
        /// 从一组 EffectConfig 创建 EffectRequest 并提交到 Buffer。
        /// </summary>
        public static void Submit(
            EffectRequestBuffer buffer,
            Entity source,
            Entity instigator,
            Entity target,
            EffectConfig[] effects,
            Vector3 hitPoint = default,
            Vector3 direction = default)
        {
            if (effects == null) return;

            for (int i = 0; i < effects.Length; i++)
            {
                var cfg = effects[i];
                var req = new EffectRequest();

                req.Type = cfg.Type;
                req.Source = source;
                req.Instigator = instigator;
                req.Target = target;
                req.Value = cfg.Value;
                req.DamageType = cfg.DamageType;
                req.ReferenceId = cfg.ReferenceId;
                req.HitPoint = hitPoint;
                req.Direction = direction;

                buffer.Submit(req);
            }
        }
    }

    [Serializable]
    public struct EffectGroup
    {
        public PayloadTrigger Trigger;
        public EffectConfig[] Effects;
    }

    public enum PayloadTrigger
    {
        /// <summary>
        /// Bullet 命中 / 主动触发
        /// </summary>
        OnHit,
        /// <summary>
        /// 目标进入区域
        /// </summary>
        OnEnter,
        /// <summary>
        /// 周期性触发
        /// </summary>
        OnTick,
        /// <summary>
        /// 目标离开区域
        /// </summary>
        OnExit,
        /// <summary>
        /// 生命周期结束时
        /// </summary>
        OnExpire,
        /// <summary>
        /// Buff 施加时
        /// </summary>
        OnApply,
        /// <summary>
        /// Buff 移除时
        /// </summary>
        OnRemove,
        /// <summary>
        /// Buff 叠层阈值触发
        /// </summary>
        OnThreshold,
    }
}
