using System.Collections;
using System.Collections.Generic;
using Frame;
using UnityEngine;

namespace Ability
{
    public class BulletManager : IManager
    {
        private LinkedList<BulletModel> actorList;
        private Dictionary<int, BulletModel> actorDict;

        public override void Enter()
        {
            base.Enter();

            actorList = new();
            actorDict = new();
        }

        public override void Exit()
        {
            foreach (var item in actorList)
            {
                item.Exit();
            }
            actorList.Clear();
            actorDict.Clear();

            base.Exit();
        }


        public void RemoveActor(int id)
        {
            if (!actorDict.ContainsKey(id))
            {
                Debugger.LogError($"actor id 不存在", LogDomain.Actor);
                return;
            }

            var actorModel = actorDict[id];
            actorModel.Exit();
            actorDict.Remove(id);
            actorList.Remove(actorModel);
        }

        public BulletModel GetActor(int id)
        {
            if (!actorDict.ContainsKey(id))
                return null;
            return actorDict[id];
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            var node = actorList.First;
            while (node != null)
            {
                node.Value.Tick(deltaTime);
                node = node.Next;
            }
        }
    }
}

