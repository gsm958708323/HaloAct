using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ability
{
    [CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/AbilityBehavior")]
    /// <summary>
    /// 存储行为数据：定义此行为将要执行的动作
    /// </summary>
    public class AbilityBehavior : SerializedScriptableObject
    {
        /// <summary>
        /// 动作列表
        /// </summary>
        /// <returns></returns>
        public List<AbilityAction> Actions = new();

        public int FrameLength = 60;
        public bool IsLoop;
        public KeyCode InputKey;
    }

}