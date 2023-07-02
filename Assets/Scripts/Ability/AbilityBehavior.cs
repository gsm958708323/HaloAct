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
    public class AbilityBehavior : SerializedScriptableObject, ILogicT<AbilityBehaviorTree>
    {
        /// <summary>
        /// 动作列表
        /// </summary>
        /// <returns></returns>
        public List<AbilityAction> Actions = new();

        public int FrameLength = 60;
        public bool IsLoop;
        public KeyCode InputKey;



        protected AbilityBehaviorTree tree;

        public virtual void Init()
        {
        }

        public virtual void Enter(AbilityBehaviorTree tree)
        {
            Debugger.Log($"Enter {name} {GetType()}", LogDomain.AbilityBehavior);
            this.tree = tree;
        }

        public virtual void Exit()
        {
            Debugger.Log($"Exit {name} {GetType()}", LogDomain.AbilityBehavior);
            this.tree = null;
        }

        public virtual void Tick(int curFrame)
        {
            UpdateActions(curFrame);
            UpdateAttack(curFrame);
        }
        
        private void UpdateAttack(int curFrame)
        {

        }

        private void UpdateActions(int curFrame)
        {
            foreach (var action in Actions)
            {
                if (action != null)
                {
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
            }
        }


    }
}