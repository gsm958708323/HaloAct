using System;

namespace Combat
{
    public class EffectPayloadComponent : IComponent
    {
        public EffectGroup[] Groups;
    }

    public static class PayloadHelper
    {
        public static EffectConfig[] GetEffects(
            EffectPayloadComponent payload, PayloadTrigger trigger)
        {
            if (payload?.Groups == null) return Array.Empty<EffectConfig>();

            for (int i = 0; i < payload.Groups.Length; i++)
            {
                if (payload.Groups[i].Trigger == trigger)
                    return payload.Groups[i].Effects;
            }
            return Array.Empty<EffectConfig>();
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
