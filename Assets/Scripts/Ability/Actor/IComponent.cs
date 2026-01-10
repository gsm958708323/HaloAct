using System.Collections;
using System.Collections.Generic;

namespace Ability
{
    public abstract class IComponent
    {
        public virtual void Enter(IEntity entity)
        {

        }

        public virtual void Exit()
        {

        }

        public virtual void Init()
        {

        }

        public virtual void Destroy()
        {

        }

        public virtual void Tick(float deltaTime)
        {

        }
    }

    public class ComponentLogic : IComponent
    {

        protected Entity entity;
        public override void Enter(IEntity entity)
        {
            this.entity = entity as Entity;
        }
    }

    public class ComponentRender : IComponent
    {
        protected EntityRender entityRender;
        public override void Enter(IEntity entity)
        {
            entityRender = entity as EntityRender;
        }
    }
}

