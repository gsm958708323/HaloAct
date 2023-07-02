using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ability
{
    [CreateAssetMenu(fileName = "NewNode", menuName = "AbilityTree/AbilityNode")]
    /// <summary>
    /// 记录行为节点的连招过渡关系和其他拓展信息
    /// </summary>
    public class AbilityNode : SerializedScriptableObject, ILogicT<AbilityBehaviorTree>
    {
        /// <summary>
        /// 当前节点的唯一标识，BehaviorIndex可重复，但IId是唯一的
        /// </summary>
        public int Id;
        /// <summary>
        /// 指向叶子节点的Id
        /// </summary>
        /// <returns></returns>
        public List<int> Childs = new();
        /// <summary>
        /// 条件检查
        /// </summary>
        /// <returns></returns>
        public List<AbilityCondition> conditions = new();

        public int Priority;
        /// <summary>
        /// 行为配置的索引
        /// </summary>
        public int BehaviorIndex;
        /// <summary>
        /// 当前引用的行为节点
        /// </summary>
        public AbilityBehavior curBehavior;
        protected AbilityBehaviorTree tree;

        /// <summary>
        /// 当前行为是否可以打断
        /// </summary>
        public bool CanCancel;

        public virtual void Init()
        {
        }
        public virtual void Enter(AbilityBehaviorTree tree)
        {
            Debugger.Log($"Enter {name} {GetType()}", LogDomain.AbilityNode);
            this.tree = tree;
            this.curBehavior = tree.GetAbilityBehavior(BehaviorIndex);
            CanCancel = false;

            curBehavior?.Enter(tree);
        }

        public virtual void Exit()
        {
            Debugger.Log($"Exit {name} {GetType()}", LogDomain.AbilityNode);
            curBehavior?.Exit();

            this.tree = null;
            this.curBehavior = null;
        }

        public virtual void Tick(int curFrame)
        {
            curBehavior?.Tick(curFrame);
        }

        internal bool CheckCondition(AbilityBehaviorTree tree)
        {
            foreach (var item in conditions)
            {
                if (!item.Check(tree))
                {
                    return false;
                }
            }
            return true;
        }
    }
}