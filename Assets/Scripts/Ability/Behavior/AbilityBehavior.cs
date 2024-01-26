using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Ability
{
    /// <summary>
    /// 存储行为数据：定义此行为将要执行的动作
    /// </summary>
    public abstract class AbilityBehavior : BehaviorBase
    {
        public int FrameLength = 60;
        public bool IsLoop;
        public KeyCode InputKey;
        /// <summary>
        /// 格挡角度
        /// </summary>
        /// <value></value>
        public float BlockAngle { get; internal set; }
    }
}