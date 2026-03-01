
using UnityEngine;

namespace Ability
{
    public class RenderTransformComp : ComponentRender
    {
        EntityRender render;
        CharacterController controller;

        public override void Enter(IEntity entity)
        {
            base.Enter(entity);
            controller = entityRender.gameObject.GetComponent<CharacterController>();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (entityRender == null || entityRender.LogicEntity == null)
                return;

            Vector3 pos;
            Quaternion rot;

            var transComp = entityRender.LogicEntity.GetComp<TransfromComp>();
            if (transComp is not null)
            {
                pos = transComp.Position;
                rot = transComp.Rotation;
            }
            else
            {
                var simpleTrans = entityRender.LogicEntity.GetComp<SimpleTransformComp>();
                if (simpleTrans is null)
                    return;

                pos = simpleTrans.Position;
                rot = simpleTrans.Rotation;
            }

            if (controller != null)
            {
                controller.transform.position = pos;
                controller.transform.rotation = rot;
            }
            else
            {
                entityRender.gameObject.transform.position = pos;
                entityRender.gameObject.transform.rotation = rot;
            }
        }
    }
}
