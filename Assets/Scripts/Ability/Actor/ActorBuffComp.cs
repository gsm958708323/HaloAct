using System.Collections.Generic;
using Ability;
using Frame;
using UnityEditor;
using UnityEngine;

namespace Ability
{
    public class ActorBuffComp : ILogicT<ActorModel>
    {
        List<BuffModel> buffList;
        Dictionary<int, BuffModel> buffDict;
        ActorModel actor;

        public void Init()
        {
        }

        public void Enter(ActorModel t)
        {
            buffList = new();
            buffDict = new();
            actor = t;
        }

        public void Exit()
        {
            buffList = null;
            buffDict = null;
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                var buffModel = buffList[i];
                buffModel.Tick(deltaTime);
            }
        }

        public BuffModel AddBuff(AddBuffInfo addInfo)
        {
            var buffId = addInfo.Id;

            // buff静态配置，buff添加时的动态配置

            // 同一个角色身上，只有有一个id相同的buff实例
            var buffModel = GetBuffById(addInfo.Id);
            if (buffModel is null)
            {
                var path = $"Actor/{buffId}";
                var buffData = Resources.Load<BuffData>(path);
                if (buffData is null)
                {
                    Debugger.LogError($"actor配置不存在 {path}", LogDomain.Buff);
                    return null;
                }

                buffModel = new BuffModel();
                buffModel.Init();
                buffModel.Enter(buffData);

                buffList.Add(buffModel);
                buffList.Sort((a, b) =>
                {
                    return a.BuffData.Priority.CompareTo(b.BuffData.Priority);
                });
                buffDict[buffId] = buffModel;
            }
            else
            {
                // 处理堆叠逻辑
                var buffData = buffModel.BuffData;
                buffModel.ModLifetime(addInfo.ModLifetime);
                buffModel.ModStack(addInfo.ModStack);
                buffModel.Permanent = addInfo.Permanent;

                if (buffModel.Stack > 0)
                {
                    buffData?.onOccur.Enter(actor.Behavior);
                }
            }

            return buffModel;
        }

        /// <summary>
        /// 判断是否拥有某个buff： buffId相同，施法者相同
        /// todo：测试buffid相同，但是施法者不同的情况
        /// </summary>
        /// <param name="buffInfo"></param>
        /// <returns></returns>
        private BuffModel GetBuffById(int buffId)
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
    /// 存放添加buff时的环境数据
    /// </summary>
    public struct AddBuffInfo
    {
        public int Id;
        public ActorModel Creater;
        public int Target;

        /// <summary>
        /// 修改堆叠次数，正负数
        /// </summary>
        public int ModStack { get; internal set; }
        public float ModLifetime { get; internal set; }
        public bool Permanent { get; internal set; }
    }
}
