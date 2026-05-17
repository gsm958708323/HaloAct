using System.Collections.Generic;

namespace Combat
{
    public class BuffInstance
    {
        // 身份
        public int InstanceId;          // 运行时唯一标识（自增分配）
        public int ConfigId;
        public Entity Source;           // 施加者
        public Entity Owner;            // 宿主

        // 生命周期
        public float Duration;
        public float Remaining;

        // 叠层
        public int CurrentStacks;
        public int MaxStacks;
        public StackMode StackMode;

        // 数值修改
        public List<Modifier> Modifiers;

        // 周期效果
        public float TickInterval;
        public float TickTimer;
        public EffectGroup[] PayloadGroups; // 复用 EffectGroup 结构

        // 条件触发
        public List<TriggerRule> TriggerRules;

        // 互斥 / 免疫
        public BuffGroupTag GroupTag;
        public int Priority;
        public BuffGroupTag[] GrantsImmunityTo;

        // 依赖
        public int DependsOnBuffId;

        public RefreshPolicy RefreshPolicy { get; internal set; }
        public int StackThreshold { get; internal set; }
        public ThresholdAction ThresholdAction { get; internal set; }
        public bool IsShield { get; internal set; }
        public float ShieldValue { get; internal set; }
        public DamageType ShieldAbsorbType { get; internal set; }
    }
}