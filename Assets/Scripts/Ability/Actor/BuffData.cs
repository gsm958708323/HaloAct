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
    [CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/BuffData")]
    public class BuffData : SerializedScriptableObject
    {
        [MinValue(1)]
        public int Id;
        public string Name;
        /// <summary>
        /// buff优先级，优先级越低越晚执行
        /// </summary>
        public int Priority;
        /// <summary>
        /// buff堆叠中的最大层数，id和caster相同则可以堆叠
        /// </summary>
        [MinValue(1)]
        public int MaxStack;
        /// <summary>
        /// 多久调用一次onTick函数（单位：s）
        /// </summary>
        [MinValue(0f)]
        public float TickTimeInterval;
        /// <summary>
        /// buff的标签，用于筛选
        /// </summary>
        public string[] Tag;
        public BuffModifierGroup Modifier = BuffModifierGroup.CreateDefault();

        /// <summary>
        /// buff释放时调用
        /// </summary>
        public BuffCastAction OnCast;
        /// <summary>
        /// 每次到达tickTime时调用
        /// </summary>
        public BuffTickAction OnTick;
        /// <summary>
        /// buff拥有者造成伤害时调用
        /// </summary>
        public BuffHitAction OnHit;
        /// <summary>
        /// buff拥有者受到伤害时调用
        /// </summary>
        public BuffBeHurtAction OnBeHurt;
        /// <summary>
        /// buff拥有者击杀目标时调用
        /// </summary>
        public BuffKillAction OnKill;
        /// <summary>
        /// buff拥有者被击杀时调用
        /// </summary>
        public BuffBeKilledAction OnBeKilled;
        /// <summary>
        ///buff被移除之前要做的事情
        /// </summary>
        public BuffRemovedAction OnRemoved;
        /// <summary>
        /// buff在被添加，改变层数时触发
        /// </summary>
        public BuffOccurAction OnOccur;

        public bool HasTimedTick => TickTimeInterval > 0f;
    }
}
