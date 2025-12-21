using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ability
{
    /// <summary>
    /// buff数据处理，存储动态数据和配置
    /// </summary>
    public class EffectObj : ILogicT<BuffData>
    {
        public BuffData BuffData;
        public Entity Creater;
        public Entity Target;

        public bool Permanent;
        public int Stack { get; private set; }

        /// <summary>
        /// 用来计算每次OnTick调用
        /// </summary>
        private float tickTime;

        /// <summary>
        /// 运行时间
        /// </summary>
        private float timeElapsed;
        /// <summary>
        /// 剩余多久失效
        /// </summary>
        private float duration;

        public void Enter(BuffData t)
        {
            BuffData = t;
        }

        public void Exit()
        {
            BuffData = null;
        }

        public void Init()
        {

        }

        public void Tick(float deltaTime) { }

        /// <summary>
        /// 生命周期是否结束
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public bool TickFinish(float deltaTime)
        {
            if (!Permanent)
            {
                duration -= deltaTime;
            }
            timeElapsed += deltaTime;
            tickTime += deltaTime;
            if (tickTime >= BuffData.TickTimeInterval)
            {
                tickTime -= BuffData.TickTimeInterval;
                BuffData.OnTick.Execute(this);
            }

            if (duration <= 0 || Stack <= 0)
            {
                BuffData.OnRemoved?.Execute(this);
                return true;
            }
            return false;
        }

        internal void ModDuration(float add, bool isOveriDuration)
        {
            duration = isOveriDuration ? add : duration + add;
            if (duration < 0)
            {
                duration = 0;
            }
        }

        /// <summary>
        /// 修改堆叠次数，返回值
        /// </summary>
        /// <param name="add"></param>
        /// <returns></returns>
        internal int ModStack(int add)
        {
            // 分别考虑modStack正负，finalStack是否大于max和小于0
            var finalStack = Stack + add;
            finalStack = Math.Clamp(finalStack, 0, BuffData.MaxStack);

            var oldStack = Stack;
            Stack = finalStack;
            return finalStack - oldStack;
        }
    }
}