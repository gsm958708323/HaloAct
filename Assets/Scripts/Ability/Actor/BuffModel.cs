using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Unity.VisualScripting;

namespace Ability
{
    /// <summary>
    /// buff数据处理，存储动态数据
    /// </summary>
    public class BuffModel : ILogicT<BuffData>
    {
        public BuffData BuffData;
        public ActorModel Creater;
        public ActorModel Target;

        public bool Permanent;
        public float Lifetime { get; private set; }
        public int Stack { get; private set; }

        public void Enter(BuffData t)
        {
            BuffData = t;
            Stack = 1;
            Lifetime = BuffData.Lifetime;
            Permanent = BuffData.Permanent;
        }

        public void Exit()
        {
            BuffData = null;
            Lifetime = 0;
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void Tick(float deltaTime)
        {
            throw new NotImplementedException();
        }

        internal void ModLifetime(float modLifetime)
        {
            Lifetime += modLifetime;
            if (Lifetime < 0)
            {
                Lifetime = 0;
            }
        }


        /// <summary>
        /// 修改堆叠次数，返回值
        /// </summary>
        /// <param name="modStack"></param>
        /// <returns></returns>
        internal int ModStack(int modStack)
        {
            // 分别考虑modStack正负，finalStack是否大于max和小于0
            var finalStack = Stack + modStack;
            finalStack = Math.Clamp(finalStack, 0, BuffData.MaxStack);

            var oldStack = Stack;
            Stack = finalStack;
            return finalStack - oldStack;
        }
    }
}