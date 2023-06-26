using System;
using System.Collections;
using System.Collections.Generic;
using NodeCanvas.Tasks.Actions;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 技能行为树
    /// 管理关系：AbilityBehaviorTree -> AbilityNode -> AbilityBehavior -> AbilityAction 
    /// </summary>
    public class AbilityBehaviorTree
    {
        /// <summary>
        /// 当前行为的帧计数
        /// </summary>
        public int curFrame;
        /// <summary>
        /// 当前进行的行为节点
        /// </summary>
        public AbilityNode curBehavior;
        /// <summary>
        /// 当前执行的叶子节点的索引
        /// </summary>
        public int curBehaviorIndex;
        /// <summary>
        /// 存储所有的节点
        /// </summary>
        /// <returns></returns>
        public List<AbilityNode> behaviorsList = new();

        float fps;
        float cacheTime;
        ActorModel actorModel;

        public AbilityBehaviorTree(ActorModel model)
        {
            actorModel = model;
            fps = 1.0f / GameManager_Settings.TargetFraneRate;
        }

        public void OnUpdate()
        {
            if (curBehavior == null)
                return;
            cacheTime += Time.deltaTime;

            // 超过fps执行一次Tick
            while (cacheTime > fps)
            {
                if (curFrame >= curBehavior.FrameLength)
                {
                    if (curBehavior.IsLoop)
                    {
                        LoopBehavior();
                    }
                    else
                    {
                        EndBehavior();
                    }
                }
                curFrame += 1;
                UpdateActions();
                UpdateAttack();

                cacheTime -= fps;
            }
        }

        private void UpdateAttack()
        {
            throw new NotImplementedException();
        }

        private void UpdateActions()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 将行为重置到第一帧
        /// </summary>
        private void LoopBehavior()
        {
            curFrame = 0;
        }

        private void EndBehavior()
        {
            StartBehavior(GetBehavior("Default"));
        }

        private AbilityNode GetBehavior(string name)
        {
            foreach (var item in behaviorsList)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }

            Debug.LogError($"行为不存在 {name}");
            return null;
        }

        private bool CheckNextBehavior()
        {
            if (behaviorsList.Count == 0)
            {
                Debug.LogError($"没有可选择的行为列表");
                return false;
            }

            if (curBehaviorIndex >= behaviorsList.Count)
            {
                curBehaviorIndex = 0;
            }

            var curBehavior = behaviorsList[curBehaviorIndex];
            foreach (var newBehaviorIndex in curBehavior.Childs)
            {
                var newBehavior = behaviorsList[newBehaviorIndex];
            }
            return false;
        }

        public void StartBehavior(AbilityNode newBehavior)
        {
            if (newBehavior == null)
                return;

            ResetBehavior(newBehavior);
            curFrame = 0;
            curBehavior = newBehavior;

            if (curBehavior == GetBehavior("Default"))
            {
                curBehaviorIndex = 0;
            }
            actorModel.CanCancel = false;
        }

        private void ResetBehavior(AbilityNode newBehavior)
        {
            foreach (var item in newBehavior.Actions)
            {
                item.OnExit(actorModel);
            }
        }
    }
}

