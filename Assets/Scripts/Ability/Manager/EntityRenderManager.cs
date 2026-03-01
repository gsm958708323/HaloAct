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
            var playerDataComp = entity.GetComp<PlayerDataComp>();
            if (playerDataComp is not null)
            {
                var actorRender = AddRenderEntity(entity);
                var data = playerDataComp.Data;
                var actorGo = GameObject.Instantiate(data.Prefab);

                var actorTrans = entity.GetComp<TransfromComp>();
                if (actorTrans is not null)
                {
                    actorGo.transform.position = actorTrans.Position;
                    actorGo.transform.rotation = actorTrans.Rotation;
                }

                actorRender.BindGo(actorGo);
                actorRender.AddComp<RenderTransformComp>();

                var hurtBoxes = actorGo.GetComponentsInChildren<HurtBox>(true);
                for (int i = 0; i < hurtBoxes.Length; i++)
                {
                    hurtBoxes[i].Enter(entity);
                }
                return;
            }

            var bulletComp = entity.GetComp<BulletDataComp>();
            if (bulletComp is null || bulletComp.Data is null)
                return;

            var bulletRender = AddRenderEntity(entity);
            var bulletGo = GameObject.Instantiate(bulletComp.Data.Prefab);

            var bulletTrans = entity.GetComp<SimpleTransformComp>();
            if (bulletTrans is not null)
            {
                bulletGo.transform.position = bulletTrans.Position;
                bulletGo.transform.rotation = bulletTrans.Rotation;
            }

            bulletRender.BindGo(bulletGo);
            bulletRender.AddComp<RenderTransformComp>();

            var bulletHurtBoxes = bulletGo.GetComponentsInChildren<HurtBox>(true);
            for (int i = 0; i < bulletHurtBoxes.Length; i++)
            {
                bulletHurtBoxes[i].Enter(entity);
            }

            var scale = bulletComp.Data.Radius > 0 ? Vector3.one * (bulletComp.Data.Radius * 2f) : Vector3.one;
            var hitInfo = new HitBoxInfo
            {
                HitBoxPos = Vector3.zero,
                HitBoxRot = Quaternion.identity,
                HitBoxScale = scale
            };

            UnityGameAPI.InitHitBox(entity.Uid, hitInfo, other => FightManager.Bullet.OnHit(entity.Uid, other));
            bulletComp.Data.OnCreate?.Execute();
        }
    }
}
