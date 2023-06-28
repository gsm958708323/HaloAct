using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Tasks.Actions;
using UnityEditor;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 技能行为树
    /// 管理关系：AbilityBehaviorTree -> AbilityNode -> AbilityBehavior -> AbilityAction 
    /// </summary>
    public class AbilityBehaviorTree : ILogic
    {
        /// <summary>
        /// 当前行为的帧计数
        /// </summary>
        public int curFrame;
        /// <summary>
        /// 当前进行的行为节点
        /// </summary>
        public AbilityBehavior curBehavior;
        /// <summary>
        /// 当前执行的行为节点的索引
        /// </summary>
        public int curNodeIndex;
        public List<AbilityNode> nodeList = new();
        public List<AbilityBehavior> behaviorsList = new();

        float fps;
        float cacheTime;
        ActorModel actorModel;

        public AbilityBehaviorTree(ActorModel model)
        {
            actorModel = model;
            fps = 1.0f / GameManager_Settings.TargetFraneRate;
        }

        public void Init() { }

        public void Init(string nodePath, string behaviorPath)
        {
            
            LoadNode(nodePath);
            LoadBehavior(behaviorPath);
        }

        private void LoadNode(string nodePath)
        {
            nodeList = Resources.LoadAll<AbilityNode>(nodePath).ToList();
            if (nodeList.Count == 0)
            {
                Debug.LogError("行为节点初始化错误");
                return;
            }
        }

        private void LoadBehavior(string behaviorPath)
        {
            behaviorsList = Resources.LoadAll<AbilityBehavior>(behaviorPath).ToList();
            if (behaviorsList.Count == 0)
            {
                Debug.LogError("行为数据初始化错误");
                return;
            }

            foreach (var behavior in behaviorsList)
            {
                foreach (var action in behavior.Actions)
                {
                    action?.Init(actorModel);
                }
            }
        }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public void Tick()
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

        }

        private void UpdateActions()
        {
            foreach (var action in curBehavior.Actions)
            {
                if (action != null)
                {
                    int startFrame = action.StartFrame + 1;
                    int endFrame = action.EndFrame + 1;

                    if (curFrame == startFrame)
                    {
                        action.Enter(actorModel);
                    }
                    else if (curFrame > startFrame && curFrame < endFrame)
                    {
                        action.Tick(actorModel);
                    }
                    else if (curFrame == endFrame)
                    {
                        action.Exit(actorModel);
                    }
                }
            }
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

        private AbilityBehavior GetBehavior(string name)
        {
            foreach (var item in behaviorsList)
            {
                if (item.name == name)
                {
                    return item;
                }
            }

            Debug.LogError($"行为不存在 {name}");
            return null;
        }

        private AbilityBehavior TryGetNextBehavior()
        {
            if (nodeList.Count == 0)
            {
                Debug.LogError($"没有可选择的行为节点");
                return null;
            }

            if (curNodeIndex >= behaviorsList.Count)
            {
                curNodeIndex = 0;
            }

            AbilityNode curNode = nodeList[curNodeIndex];
            int priority = -1;
            AbilityNode nextNode = default;
            foreach (var newNodeIndex in curNode.Childs)
            {
                AbilityNode newNode = nodeList[newNodeIndex];
                AbilityBehavior behavior = behaviorsList[newNode.BehaviorIndex];
                // 检查输入
                if (GameManager_Input.Instance.bufferKeys.Any(predicate => predicate == behavior.InputKey))
                {
                    // 检查条件
                    if (behavior.CheckCondition(actorModel))
                    {
                        if (newNode.Priority > priority)
                        {
                            priority = newNode.Priority;
                            nextNode = newNode;
                        }
                    }
                }
            }
            if (priority > -1)
            {
                curNodeIndex = nextNode.BehaviorIndex;
                return behaviorsList[curNodeIndex];
            }

            return null;
        }

        public void StartBehavior(AbilityBehavior newBehavior)
        {
            if (newBehavior == null)
                return;

            ResetBehavior(newBehavior);
            curFrame = 0;
            curBehavior = newBehavior;

            if (curBehavior == GetBehavior("Default"))
            {
                curNodeIndex = 0;
            }
            actorModel.CanCancel = false;
        }

        private void ResetBehavior(AbilityBehavior newBehavior)
        {
            foreach (var item in newBehavior.Actions)
            {
                item.Exit(actorModel);
            }
        }

    }
}

