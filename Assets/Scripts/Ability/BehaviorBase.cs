using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Ability
{
    public interface IAbilityAction
    {
        public void Enter(AbilityBehaviorTree tree);
    }

    /// <summary>
    /// 存储行为数据：定义此行为将要执行的动作
    /// </summary>
    public abstract class BehaviorBase : SerializedScriptableObject, ILogicT<AbilityBehaviorTree>
    {
        public int FrameLength = 60;

        /// <summary>
        /// 动作列表
        /// </summary>
        /// <returns></returns>
        public List<IAbilityAction> Actions = new();
        protected AbilityBehaviorTree tree;

        public virtual void Init()
        {
        }

        public virtual void Enter(AbilityBehaviorTree tree)
        {
            this.tree = tree;
            Debugger.Log($"Enter {tree.ActorModel.name} {name} {GetType()}", LogDomain.AbilityBehavior);
        }

        public virtual void Exit()
        {
            Debugger.Log($"Exit {tree.ActorModel.name} {name} {GetType()}", LogDomain.AbilityBehavior);
            foreach (var actionT in Actions)
            {
                if (actionT is null) continue;
                if (actionT is AbilityAction)
                {
                    var action = actionT as AbilityAction;
                    if (action.IsEnter())
                    {
                        action.Exit();
                    }
                }
            }
            this.tree = null;
        }

        public virtual void Tick(float deltaTime)
        {
            UpdateActions(tree.curFrame);
            // UpdateAttack(curFrame);
        }

        private void UpdateActions(int curFrame)
        {
            foreach (var actionT in Actions)
            {
                if (actionT is null) continue;
                if (actionT is AbilityAction)
                {
                    var action = actionT as AbilityAction;
                    int startFrame = action.StartFrame;
                    int endFrame = action.EndFrame;

                    if (curFrame == startFrame)
                    {
                        action.Enter(tree);
                    }
                    if (curFrame >= startFrame && curFrame <= endFrame)
                    {
                        action.Tick(curFrame);
                    }
                    if (curFrame == endFrame)
                    {
                        action.Exit();
                    }
                }
                else if (actionT is AbilitySimpleAction)
                {
                    var action = actionT as AbilitySimpleAction;
                    int startFrame = action.StartFrame;
                    if (curFrame == startFrame)
                    {
                        action.Enter(tree);
                    }
                }
            }
        }
    }
}