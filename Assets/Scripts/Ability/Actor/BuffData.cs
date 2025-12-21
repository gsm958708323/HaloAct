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
        public int Id;
        public string Name;
        /// <summary>
        /// buff优先级，优先级越低越晚执行
        /// </summary>
        public int Priority;
        /// <summary>
        /// buff堆叠中的最大层数，id和caster相同则可以堆叠
        /// </summary>
        public int MaxStack;
        /// <summary>
        /// 多久调用一次onTick函数（单位：s）
        /// </summary>
        public float TickTimeInterval;
        /// <summary>
        /// buff的标签，用于筛选
        /// </summary>
        public string[] Tag;

        /// <summary>
        /// buff释放时调用
        /// </summary>
        public BuffCastAction OnCast;
        /// <summary>
        /// 每次到达tickTime时调用
        /// </summary>
        public BuffTickAction OnTick;
        /// <summary>
        ///buff被移除之前要做的事情
        /// </summary>
        public BuffRemovedAction OnRemoved;
        /// <summary>
        /// buff在被添加，改变层数时触发
        /// </summary>
        public BuffOccurAction OnOccur;
    }
}