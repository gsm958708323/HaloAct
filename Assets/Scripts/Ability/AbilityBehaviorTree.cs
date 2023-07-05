using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 技能行为树
    ///                                                               -> AbilityAction 
    /// 管理关系：AbilityBehaviorTree -> AbilityNode -> AbilityBehavior 
    ///                                                               -> AbilityCondition
    /// </summary>
    public class AbilityBehaviorTree : ILogicT<ActorModel>
    {
        /// <summary>
        /// 当前行为的帧计数
        /// </summary>
        int curFrame;
        /// <summary>
        /// 当前执行的行为节点的索引（这里Index和Id是相等的）
        /// </summary>
        int curNodeIndex;
        List<AbilityNode> nodeList = new();
        List<AbilityBehavior> behaviorsList = new();
        // /// <summary>
        // /// 当前行为是否可以打断
        // /// </summary>
        // public bool CanCancel;
        public ActorModel ActorModel;
        public AbilityNode curNode;

        public void Init() { }

        public void Init(string nodePath, string behaviorPath)
        {

            LoadBehavior(behaviorPath);
            LoadNode(nodePath);

            StartBehavior(GetBehaviorById(0));
        }


        public void Enter(ActorModel model)
        {
            this.ActorModel = model;
        }

        public void Exit()
        {
            ActorModel = null;
        }

        public void Tick(int frame)
        {
            AbilityNode nextBehavior = TryGetNextBehavior();
            if (nextBehavior != null)
            {
                StartBehavior(nextBehavior);
            }

            curNode.Tick(curFrame);
            curFrame += 1;
            Debugger.Log($"{curFrame}", LogDomain.Frame);

            // 执行次数？生命周期完整？重置之后curFrame是否正确？
            if (curFrame > curNode.curBehavior.FrameLength)
            {
                if (curNode.curBehavior.IsLoop)
                {
                    LoopBehavior();
                }
                else
                {
                    EndBehavior();
                }
            }
        }

        private void LoadNode(string nodePath)
        {
            nodeList = Resources.LoadAll<AbilityNode>(nodePath).ToList();
            if (nodeList.Count == 0)
            {
                Debug.LogError("行为节点初始化错误");
                return;
            }
            nodeList.Sort((x, y) => x.Id.CompareTo(y.Id));

            //设置Node和Behavior的对应关系 
            var name2Index = new Dictionary<string, int>();
            for (int i = 0; i < behaviorsList.Count; i++)
            {
                name2Index[behaviorsList[i].name] = i;
            }
            foreach (var item in nodeList)
            {
                var nameT = Regex.Replace(item.name, @"\d", ""); // Dash1，Dash2，Dash3 只对比Dash
                var isGet = name2Index.TryGetValue(item.name, out int index) || name2Index.TryGetValue(nameT, out index);
                if (isGet)
                {
                    item.BehaviorIndex = index;
                }
                else
                {
                    Debug.LogError($"设置Node和Behavior的对应关系错误 {item.name}");
                }
                item?.Init();
            }
        }

        public AbilityBehavior GetAbilityBehavior(int index)
        {
            if (index < 0 || index >= behaviorsList.Count)
            {
                return null;
            }

            return behaviorsList[index];
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
                behavior?.Init();
                foreach (var action in behavior.Actions)
                {
                    action?.Init();
                }
                foreach (var attack in behavior.Attacks)
                {
                    attack?.Init();
                }
            }
        }


        /// <summary>
        /// 将行为重置到第一帧
        /// </summary>
        private void LoopBehavior()
        {
            curFrame = 1;
            // CanCancel = false;
        }

        private void EndBehavior()
        {
            StartBehavior(GetBehaviorById(0));
        }

        private AbilityNode GetBehaviorById(int id)
        {
            // 判断数组越界
            if (id < 0 || id >= nodeList.Count)
            {
                Debug.LogError($"行为节点不存在 {id}");
                return null;
            }

            return nodeList[id];
        }

        private AbilityNode TryGetNextBehavior()
        {
            if (nodeList.Count == 0)
            {
                Debug.LogError($"没有可选择的行为节点");
                return null;
            }

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
                    if (newNode.CheckCondition(this))
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
                return nextNode;
            }

            return null;
        }

        public void StartBehavior(AbilityNode newNode)
        {
            if (newNode == null || newNode == curNode)
                return;

            curFrame = 1;
            curNode?.Exit();
            curNode = newNode;
            newNode?.Enter(this);
            // CanCancel = false;
        }

        private void ResetBehavior(AbilityBehavior behavior)
        {
            foreach (var item in behavior.Actions)
            {
                item.Exit();
            }
        }

    }
}

