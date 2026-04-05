using UnityEngine;

namespace Ability
{
    public class AoeRenderTransformComp : ComponentRender
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

        void SyncFromLogic()
        {
            if (entityRender.gameObject is null)
            {
                return;
            }

            var aoeComp = entityRender.LogicEntity.GetComp<AoeDataComp>();
            if (aoeComp == null)
            {
                return;
            }

            entityRender.gameObject.transform.position = aoeComp.Position;
        }
    }
}
