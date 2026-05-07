using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    [CreateAssetMenu]
    public class BuffConfig : ScriptableObject
    {
        public int BuffId;
        public string BuffName;
        public float Duration;
        public int MaxStack;
        public int Priority;
        /// <summary>
        /// 互斥组标签
        /// </summary>
        public BuffGroupTag GroupTag;
        /// <summary>
        /// 是否为免疫类buff
        /// </summary>
        public bool Immunity;
        /// <summary>
        /// 施加时触发的效果
        /// </summary>
        public EffectConfig[] OnApplyEffects;
        public EffectConfig[] OnTickEffects;
        public EffectConfig[] OnRemoveEffects;
    }

    public enum BuffGroupTag
    {

    }
}

