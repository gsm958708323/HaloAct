using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    [CreateAssetMenu]
    public class BuffConfig : ScriptableObject
    {
        public int BuffId;
        public float Duration;
        public int MaxStack = 1;
        public StackMode StackMode;
        public RefreshPolicy RefreshPolicy;
        public int StackThreshold;
        /// <summary>
        /// 堆叠层数达到阈值后的行为
        /// </summary>
        public ThresholdAction ThresholdAction;
        public float TickInterval;
        public int Priority;
        public BuffGroupTag GroupTag;
        public BuffGroupTag[] GrantsImmunityTo;
        public int DependsOnBuffId;
        public bool IsShield;
        public float ShieldValue;
        public DamageType ShieldAbsorbType;
        public Modifier[] Modifiers;
        public TriggerRule[] TriggerRules;
        public EffectGroup[] PayloadGroups;
    }

    [Serializable]
    public struct TriggerRule
    {
        public TriggerEvent Event;
        public TriggerCondition Condition;
        public float ConditionParam;
        public DamageType RequiredDamageType;   // DamageTypeEquals 时使用
        public EffectConfig[] Effects;
    }

     public enum TriggerEvent
    {
        OnTakeDamage,
        OnDealDamage,
        OnHeal,
        OnKill,
        OnDeath,
        OnBuffApplied,
        OnBuffRemoved,
        OnShieldBreak,
    }

    public enum TriggerCondition
    {
        None,
        HealthBelow,
        HealthAbove,
        StackCountEquals,
        DamageTypeEquals,
    }

    public enum TargetRule
    {
        Self,
        DirectTarget,
        AOETargets,
        Source,
    }

    public enum RefreshPolicy
    {
        Reset,          // 重置为配置持续时间
        Max,            // 取当前剩余与配置的较大值
        Extend,         // 累加
    }

    public enum ThresholdAction
    {
        ResetStacks,
        RemoveBuff,
        Keep,
    }

    public enum BuffGroupTag
    {
        None,
        Burn,
        Freeze,
        Poison
    }
}

