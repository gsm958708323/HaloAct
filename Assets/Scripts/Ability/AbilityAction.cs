using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using System;

namespace Ability
{
    public abstract class AbilityAction : ILogicT<AbilityBehaviorTree>
    {
        public int StartFrame = 1;
        public int EndFrame = 60;
        protected AbilityBehaviorTree tree;

        public virtual void Init()
        {
        }

        public void Enter(AbilityBehaviorTree tree)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.AbilityAction);
            this.tree = tree;
            OnEnter();
        }

        public void Exit()
        {
            Debugger.Log($"Exit {GetType()}", LogDomain.AbilityAction);
            OnExit();
            this.tree = null;
        }

        public void Tick(int frame)
        {
            // 防止运行时动态添加action报错
            if (tree == null) return;
            OnTick(frame);
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExit()
        {
        }

        protected virtual void OnTick(int frame)
        {
        }
    }
}

