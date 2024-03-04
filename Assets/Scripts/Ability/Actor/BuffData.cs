using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// buff数据，存储静态数据
    /// </summary>
    [CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/BuffBehavior")]
    public class BuffData : SerializedScriptableObject
    {
        public int Id;
        public string Name;
        public int Priority;
        public int MaxStack;
        /// <summary>
        /// 多久调用一次onTick函数（单位：s）
        /// </summary>
        public float TickTime;
        public string Tag;
        /// <summary>
        /// 生命周期（剩余时间）
        /// </summary>
        public float Lifetime;
        /// <summary>
        /// 是否永久
        /// </summary>
        public bool Permanent;

        public AbilitySimpleAction onCast;
        public AbilitySimpleAction onTick;
        public AbilitySimpleAction onRemoved;
        public AbilitySimpleAction onOccur;
    }
}