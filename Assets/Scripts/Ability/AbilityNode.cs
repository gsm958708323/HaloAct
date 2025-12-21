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
    public class AbilityNode : SerializedScriptableObject, ILogicT<BehaviorComp>
    {
        /// <summary>
        /// 当前节点的唯一标识（自己定义的，不是索引），但是可以引用同一个行为
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
        /// 当前引用的行为节点
        /// </summary>
        public AbilityBehavior Behavior;
        protected BehaviorComp tree;

        /// <summary>
        /// 当前行为是否可以打断
        /// </summary>
        public bool CanCancel;

        public virtual void Init()
        {
        }
        public virtual void Enter(BehaviorComp tree)
        {
            Debugger.Log($"Enter {name} {GetType()}", LogDomain.AbilityNode);
            this.tree = tree;
            CanCancel = false;

            Behavior?.Enter(tree);
        }

        public virtual void Exit()
        {
            Debugger.Log($"Exit {name} {GetType()}", LogDomain.AbilityNode);
            Behavior?.Exit();

            this.tree = null;
        }

        public virtual void Tick(float deltaTime)
        {
            CanCancel = false;
            Behavior?.Tick(deltaTime);
        }

        internal bool CheckCondition(BehaviorComp tree)
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