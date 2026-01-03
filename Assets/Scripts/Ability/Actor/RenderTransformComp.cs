
using UnityEngine;

namespace Ability
{
    public class RenderTransformComp : IComponent
    {
        EntityRender render;
        CharacterController controller;

        public override void Enter(IEntity entity)
        {
            base.Enter(entity);
            render = entity as EntityRender;
            controller = render.gameObject.GetComponent<CharacterController>();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            var transComp = render.LogicEntity.GetComp<TransfromComp>();
            if (transComp is null)
            {
                return;
            }

            controller.transform.position = transComp.Position;
            controller.transform.rotation = transComp.Rotation;
        }
    }
}