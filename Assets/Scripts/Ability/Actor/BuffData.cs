using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// Buff行为
    /// </summary>
    [CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/BuffBehavior")]
    public class BuffData : SerializedScriptableObject
    {
        public int Id;
        public int Priority;
        public int MaxStack;
        public float TickTime;
        public string Tag;

        public AbilitySimpleAction onCast;
        public AbilitySimpleAction onTick;
        public AbilitySimpleAction onRemoved;
        public AbilitySimpleAction onOccur;
    }
}