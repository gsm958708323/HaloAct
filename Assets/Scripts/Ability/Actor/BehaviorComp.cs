using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// Frame-driven combo runner:
    /// AbilityBehaviorTree -> AbilityNode -> AbilityBehavior -> AbilityAction/AbilityCondition
    /// </summary>
    public class BehaviorComp : ComponentLogic
    {
        public int curFrame;

        readonly List<AbilityNode> nodeList = new();
        readonly List<AbilityBehavior> behaviorsList = new();
        readonly Dictionary<int, AbilityNode> nodeDict = new();
        readonly Dictionary<AttackType, AbilityNode> hurtNodeDict = new();

        public Entity Entity;
        public AbilityNode curNode;

        public override void Enter(IEntity model)
        {
            base.Enter(model);
            Entity = entity;

            var data = model.GetComp<PlayerDataComp>().Data;
            if (data?.ComboGraph == null)
            {
                Debug.LogError($"Actor '{data?.name ?? "Unknown"}' has no ComboGraph assigned.");
                return;
            }

            LoadComboGraph(data.ComboGraph);
            StartBehavior(GetNodeById(0));
        }

        public override void Tick(float deltaTime)
        {
            if (Entity.IsDead || curNode == null)
            {
                return;
            }

            var nextBehavior = TryGetNextBehavior();
            if (nextBehavior != null)
            {
                var buffComp = Entity.GetComp<EffectComp>();
                if (buffComp is not null)
                {
                    var newBehavior = buffComp.OnStartBehavior(nextBehavior);
                    if (newBehavior is not null)
                    {
                        nextBehavior = newBehavior;
                    }
                }

                StartBehavior(nextBehavior);
            }

            curNode.Tick(curFrame);
            curFrame += 1;
            Debugger.Log($"{curFrame}", LogDomain.Frame);

            if (curNode.Behavior == null)
            {
                return;
            }

            if (curFrame > curNode.Behavior.FrameLength)
            {
                if (curNode.Behavior.IsLoop)
                {
                    LoopBehavior();
                }
                else
                {
                    EndBehavior();
                }
            }
        }

        void LoadComboGraph(ActorComboGraphSO comboGraph)
        {
            nodeList.Clear();
            behaviorsList.Clear();
            nodeDict.Clear();
            hurtNodeDict.Clear();

            if (comboGraph == null)
            {
                Debug.LogError("Combo graph is null.");
                return;
            }

            for (int i = 0; i < comboGraph.Nodes.Count; i++)
            {
                var node = comboGraph.Nodes[i];
                if (node == null)
                {
                    continue;
                }

                nodeList.Add(node);
                nodeDict[node.Id] = node;
            }

            nodeList.Sort((left, right) => left.Id.CompareTo(right.Id));

            if (nodeList.Count == 0)
            {
                Debug.LogError("Combo graph has no nodes.");
                return;
            }

            CollectBehaviors(comboGraph);
            InitBehaviors();

            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                node.Init();

                if (node.Behavior is AbilityBehaviorHurt hurtBehavior)
                {
                    hurtNodeDict[hurtBehavior.AttackType] = node;
                }
            }
        }

        void CollectBehaviors(ActorComboGraphSO comboGraph)
        {
            var behaviorSet = new HashSet<AbilityBehavior>();

            for (int i = 0; i < nodeList.Count; i++)
            {
                var behavior = nodeList[i].Behavior;
                if (behavior != null && behaviorSet.Add(behavior))
                {
                    behaviorsList.Add(behavior);
                }
            }
        }

        void InitBehaviors()
        {
            if (behaviorsList.Count == 0)
            {
                Debug.LogError("Combo graph has no behaviors.");
                return;
            }

            for (int i = 0; i < behaviorsList.Count; i++)
            {
                var behavior = behaviorsList[i];
                behavior?.Init();
                foreach (var actionT in behavior.Actions)
                {
                    if (actionT is AbilityAction action)
                    {
                        action?.Init();
                    }
                }

                if (behavior is AbilityBehaviorAttack attackBehavior)
                {
                    foreach (var attack in attackBehavior.Attacks)
                    {
                        attack?.Init();
                    }
                }
            }
        }

        void LoopBehavior()
        {
            curFrame = 1;
        }

        void EndBehavior()
        {
            StartBehavior(GetNodeById(0));
            Entity.Target = null;
        }

        public AbilityNode GetNodeById(int id)
        {
            if (!nodeDict.TryGetValue(id, out var node))
            {
                Debug.LogError($"Missing combo node id: {id}");
                return null;
            }

            return node;
        }

        public AbilityNode GetHurtBehavior(AttackType attackType)
        {
            return hurtNodeDict.GetValueOrDefault(attackType);
        }

        public AbilityBehavior GetCurAbilityBehavior()
        {
            return curNode?.Behavior;
        }

        AbilityNode TryGetNextBehavior()
        {
            if (curNode == null)
            {
                Debug.LogError("No selectable combo nodes.");
                return null;
            }

            int priority = -1;
            AbilityNode nextNode = null;
            foreach (var childId in curNode.Childs)
            {
                var newNode = GetNodeById(childId);
                if (newNode == null || newNode.Behavior == null)
                {
                    continue;
                }

                if (GameManager_Input.Instance.bufferKeys.Any(predicate => predicate == newNode.Behavior.InputKey))
                {
                    if (newNode.CheckCondition(this) && newNode.Priority > priority)
                    {
                        priority = newNode.Priority;
                        nextNode = newNode;
                    }
                }
            }

            return priority > -1 ? nextNode : null;
        }

        public void StartBehavior(AbilityNode newNode)
        {
            if (newNode == null || newNode == curNode)
            {
                return;
            }

            curFrame = 1;
            curNode?.Exit();
            curNode = newNode;
            newNode.Enter(this);
        }

        void ResetBehavior(AbilityBehavior behavior)
        {
            foreach (var actionT in behavior.Actions)
            {
                if (actionT is AbilityAction action)
                {
                    action.Exit();
                }
            }
        }
    }
}
