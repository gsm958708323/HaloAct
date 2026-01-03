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
        }

        public override void Destroy()
        {
            GameManager.Dispatcher.RemoveListener<Entity>(EventId.CreateEntity, OnCreateEntity);
            base.Destroy();
        }

        private void OnCreateEntity(Entity entity)
        {
            var comp = entity.GetComp<PlayerDataComp>();
            if (comp is null)
                return;
            var data = comp.Data;

            var render = AddRenderEntity(entity.Uid);
            var actorGo = GameObject.Instantiate(data.Prefab);
            var idCard = actorGo.AddComponent<IdentitCard>();
            idCard.Uid = entity.Uid;
            render.Bind(actorGo);
            render.AddComp<RenderTransformComp>();
        }
    }
}
