using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class EffectComp : IComponent
    {
        List<EffectObj> buffList;
        Dictionary<int, EffectObj> buffDict;
        Entity actor;

        public override void Init()
        {
        }

        public override void Enter(Entity t)
        {
            buffList = new();
            buffDict = new();
            actor = t;
        }

        public override void Exit()
        {
            buffList = null;
            buffDict = null;
        }

        public override void Tick(float deltaTime)
        {
            if (actor.IsDead)
            {
                return;
            }

            var removeList = new List<EffectObj>();
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                var isFinish = buff.TickFinish(deltaTime);
                if (isFinish)
                {
                    removeList.Add(buff);
                }
            }

            if (removeList.Count > 0)
            {
                for (int i = 0; i < removeList.Count; i++)
                {
                    var buff = removeList[i];
                    buffList.Remove(buff);
                    buff.BuffData.OnRemoved?.Execute(buff);
                    buff.Exit();
                }
                AttrRecheck();
            }
        }

        public AbilityNode OnStartBehavior(AbilityNode node)
        {
            AbilityNode newNode = null;
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                if (buff.BuffData.OnCast != null)
                {
                    newNode = buff.BuffData.OnCast.Execute(buff, node);
                }
            }
            return newNode;
        }

        public EffectObj AddBuff(AddBuffInfo addInfo)
        {
            var buffId = addInfo.BuffId;

            // todo 相同配置id的buff，但是creater不同，是否可以堆叠
            // 同一个角色身上，只有一个id相同的buff实例，然后堆叠
            var buff = GetBuffById(buffId);
            BuffData buffData;
            if (buff is null)
            {
                buffData = GameManager.Config.LoadBuff(buffId);
                if (buffData is null)
                {
                    return null;
                }

                buff = new EffectObj();
                buff.Init();
                buff.Enter(buffData);

                buffList.Add(buff);
                buffList.Sort((a, b) =>
                {
                    return a.BuffData.Priority.CompareTo(b.BuffData.Priority);
                });
                buffDict[buffId] = buff;
            }
            else
            {
                // 处理堆叠逻辑
                buffData = buff.BuffData;
                buff.Enter(buffData);
                buff.ModDuration(addInfo.Duration, addInfo.IsOverrideDuration);
                buff.ModStack(addInfo.AddStack);
                buff.Permanent = addInfo.Permanent;
            }

            if (buff.Stack > 0)
            {
                buffData.OnOccur?.Execute(buff);
            }
            AttrRecheck();

            return buff;
        }

        /// <summary>
        /// 重新计算所有属性
        /// </summary>
        void AttrRecheck()
        {

        }

        /// <summary>
        /// 判断是否拥有某个buff： buffId相同，施法者相同
        /// </summary>
        /// <param name="buffInfo"></param>
        /// <returns></returns>
        private EffectObj GetBuffById(int buffId)
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                if (buff.BuffData.Id == buffId)
                {
                    return buff;
                }
            }
            return null;
        }
    }


    /// <summary>
    /// 存放添加buff时的动态数据
    /// </summary>
    public struct AddBuffInfo
    {
        /// <summary>
        /// buff配置id
        /// </summary>
        public int BuffId;
        /// <summary>
        /// buff创建者 todo
        /// </summary>
        public Entity Creater;
        /// <summary>
        /// buff目标
        /// </summary>
        public int Target;

        /// <summary>
        /// 修改堆叠次数，正负数
        /// </summary>
        public int AddStack { get; internal set; }
        /// <summary>
        /// 是否重写持续时间
        /// </summary>
        public bool IsOverrideDuration;

        /// <summary>
        /// 持续时间
        /// </summary>
        public float Duration { get; internal set; }
        /// <summary>
        /// 是否为永久型buff
        /// </summary>
        /// <value></value>
        public bool Permanent { get; internal set; }
    }
}
