using System.Collections.Generic;
using HaloFrame;

namespace Ability
{
    public class IEntityManager : IManager
    {
        protected LinkedList<IEntity> entityList;
        protected Dictionary<int, IEntity> entityDict;
        protected IdCreate idCreate = new();

        public override void Init()
        {
            base.Init();
            entityList = new();
            entityDict = new();
        }

        public override void Destroy()
        {
            entityList = null;
            entityDict = null;
            base.Destroy();
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            foreach (var item in entityList)
            {
                item.Exit();
            }
            entityList.Clear();
            entityDict.Clear();

            base.Exit();
        }

        protected Entity AddEntity()
        {
            var uid = idCreate.Get();
            Entity entity = new Entity();

            entity.Bind(uid);
            entity.Init();
            entity.Enter();
            
            entityList.AddLast(new LinkedListNode<IEntity>(entity));
            entityDict.Add(uid, entity);
            return entity;
        }

        protected EntityRender AddRenderEntity(int uid)
        {
            var entity = new EntityRender();
            entity.Bind(uid);
            entity.Init();
            entity.Enter();

            entityList.AddLast(new LinkedListNode<IEntity>(entity));
            entityDict.Add(uid, entity);
            return entity;
        }

        public void RemoveEntity(int uid)
        {
            if (!entityDict.ContainsKey(uid))
            {
                Debugger.LogError($"actor id 不存在", LogDomain.Actor);
                return;
            }

            var entity = entityDict[uid];
            entity.Exit();
            entityDict.Remove(uid);
            entityList.Remove(entity);
        }

        protected IEntity GetEntity(int uid)
        {
            if (!entityDict.ContainsKey(uid))
                return null;
            return entityDict[uid];
        }

        public void DriveEntity(float deltaTime)
        {
            var node = entityList.First;
            while (node != null)
            {
                node.Value.Tick(deltaTime);
                node = node.Next;
            }
        }
    }
}