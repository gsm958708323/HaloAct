using System.Collections.Generic;
using Ability;
using Frame;
using UnityEngine;

namespace Ability
{
    public class ActorBuffComp : ILogicT<ActorModel>
    {
        List<BuffModel> buffList;
        Dictionary<int, BuffModel> buffDict;

        public void Init()
        {
        }

        public void Enter(ActorModel t)
        {
            buffList = new();
            buffDict = new();
        }

        public void Exit()
        {
            buffList = null;
            buffDict = null;
        }

        public void Tick(float deltaTime)
        {
        }

        public BuffModel AddBuff(int buffId)
        {
            var path = $"Actor/{buffId}";
            var buffData = Resources.Load<BuffData>(path);
            if (buffData is null)
            {
                Debugger.LogError($"actor配置不存在 {path}", LogDomain.Buff);
                return null;
            }


            /*
            判断buff是否存在
                存在，则更新相应的buff对象
                不存在，则创建一个新的
                buff的堆叠逻辑：相同施法者，相同buffid，堆叠次数+1，超出maxStack时，则没有效果

                相同id的buff只存在一个，后面只增加堆叠次数
            */
            var buffModel = buffDict.GetValueOrDefault(buffId);
            if (buffModel is null)
            {
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

            }

            return buffModel;
        }
    }
}
