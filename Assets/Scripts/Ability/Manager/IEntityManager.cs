using System.Collections.Generic;
using System.Linq;
using HaloFrame;

namespace Ability
{
    public class IEntityManager : IManager
    {
        protected Dictionary<EntityType, LinkedList<IEntity>> entityTypeList;
        protected Dictionary<int, IEntity> entityDict;
        protected List<int> entityUidList;

        protected IdCreate idCreate = new();

        public override void Init()
        {
            base.Init();
            entityTypeList = new();
            entityDict = new();
            entityUidList = new();
        }

        public override void Destroy()
        {
            entityTypeList = null;
            entityDict = null;
            entityUidList = null;
            base.Destroy();
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            for (int i = 0; i < entityUidList.Count; i++)
            {
                var uid = entityUidList[i];
                var entity = entityDict[uid];
                entity?.Exit();
            }
            base.Exit();
        }

        protected Entity AddEntity(EntityType entityType)
        {
            var uid = idCreate.Get(entityType);
            Entity entity = new();
            AddToList(entity, uid, entityType);
            return entity;
        }

        protected EntityRender AddRenderEntity(Entity entity)
        {
            var render = new EntityRender();
            AddToList(render, entity.Uid, entity.EntityType);
            return render;
        }

        public LinkedList<IEntity> GetEntityLinkedList(EntityType entityType)
        {
            if(!entityTypeList.ContainsKey(entityType))
            {
                return null;
            }
            return entityTypeList[entityType];
        }

        void AddToList(IEntity entity, int uid, EntityType entityType)
        {
            entity.Bind(uid, entityType);
            entity.Init();
            entity.Enter();

            entityUidList.Add(uid);
            entityDict.Add(uid, entity);
            entityTypeList.TryGetValue(entityType, out var entityList);
            if (entityList is null)
            {
                entityList = new();
                entityTypeList.Add(entityType, entityList);
            }
            entityList.AddLast(new LinkedListNode<IEntity>(entity));
        }

        public void RemoveEntity(int uid)
        {
            if (!entityDict.ContainsKey(uid))
            {
                return;
            }

            var entity = entityDict[uid];
            entity.Exit();
            entityDict.Remove(uid);
            entityUidList.Remove(uid);

            entityTypeList.TryGetValue(entity.EntityType, out var entityList);
            if (entityList != null)
            {
                entityList.Remove(entity);
            }
        }

        protected IEntity GetEntity(int uid)
        {
            if (!entityDict.ContainsKey(uid))
                return null;
            return entityDict[uid];
        }

        public void DriveEntity(float deltaTime)
        {
            for (int i = 0; i < entityUidList.Count; i++)
            {
                var uid = entityUidList[i];
                var entity = entityDict[uid];
                entity?.Tick(deltaTime);
            }
        }
    }
}