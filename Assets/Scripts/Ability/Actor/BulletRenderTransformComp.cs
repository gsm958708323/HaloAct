using UnityEngine;

namespace Ability
{
    public class BulletRenderTransformComp : ComponentRender
    {
        public override void Enter(IEntity entity)
        {
            base.Enter(entity);
            SyncFromLogic();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            SyncFromLogic();
        }

        private void SyncFromLogic()
        {
            if (entityRender.gameObject is null)
            {
                return;
            }

            var bulletComp = entityRender.LogicEntity.GetComp<BulletDataComp>();
            if (bulletComp is null)
            {
                return;
            }

            var transform = entityRender.gameObject.transform;
            transform.position = bulletComp.Position;
            if (bulletComp.Direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(bulletComp.Direction.normalized);
            }
        }
    }
}
