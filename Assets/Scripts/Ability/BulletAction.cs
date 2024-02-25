using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using System;

namespace Ability
{
    public abstract class BulletAction : ILogicT<AbilityBehaviorTree>
    {
        public int StartFrame = 1;
        public int EndFrame = 60;
        protected AbilityBehaviorTree tree;
        private bool isEnter = false;

        public virtual void Init()
        {
        }

        public void Enter(AbilityBehaviorTree tree)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.AbilityAction);
            this.tree = tree;
            isEnter = true;
            OnEnter();
        }

        public void Exit()
        {
            Debugger.Log($"Exit {GetType()}", LogDomain.AbilityAction);
            OnExit();
            this.tree = null;
            isEnter = false;
        }

        public void Tick(float deltaTime)
        {
            // 防止运行时动态添加action报错
            if (tree == null) return;
            OnTick(deltaTime);
        }

        public bool IsEnter()
        {
            return isEnter;
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExit()
        {
        }

        protected virtual void OnTick(float deltaTime)
        {
        }
    }
}

