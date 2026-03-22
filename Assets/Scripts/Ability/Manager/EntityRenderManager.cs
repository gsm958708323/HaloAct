using System;
using HaloFrame;
using UnityEngine;

namespace Ability
{
    public class EntityRenderManager : IEntityManager
    {
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            // 渲染帧更新
            DriveEntity(deltaTime);
        }

        public new EntityRender GetEntity(int uid)
        {
            return base.GetEntity(uid) as EntityRender;
        }

        public override void Init()
        {
            base.Init();
            GameManager.Dispatcher.AddListener<Entity>(EventId.CreateEntity, OnCreateEntity, this);
            GameManager.Dispatcher.AddListener<int>(EventId.RemoveEntity, OnRemoveEntity, this);
        }

        public override void Destroy()
        {
            GameManager.Dispatcher.RemoveListener<Entity>(EventId.CreateEntity, OnCreateEntity);
            GameManager.Dispatcher.RemoveListener<int>(EventId.RemoveEntity, OnRemoveEntity);
            base.Destroy();
        }

        private void OnCreateEntity(Entity entity)
        {
            if (entity is null)
            {
                return;
            }

            var actorComp = entity.GetComp<PlayerDataComp>();
            if (actorComp is not null)
            {
                var render = AddRenderEntity(entity);
                var actorGo = GameObject.Instantiate(actorComp.Data.Prefab);
                render.BindGo(actorGo);
                render.AddComp<RenderTransformComp>();
                return;
            }

            var bulletComp = entity.GetComp<BulletDataComp>();
            if (bulletComp is not null)
            {
                var prefab = bulletComp.Data?.Prefab;
                if (prefab is null)
                {
                    return;
                }

                var render = AddRenderEntity(entity);
                var bulletGo = GameObject.Instantiate(prefab);
                render.BindGo(bulletGo);
                render.AddComp<BulletRenderTransformComp>();
            }
        }

        private void OnRemoveEntity(int uid)
        {
            RemoveEntity(uid);
        }
    }
}
