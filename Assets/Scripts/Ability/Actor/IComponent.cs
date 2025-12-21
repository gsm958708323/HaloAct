using System.Collections;
using System.Collections.Generic;

namespace Ability
{
    public abstract class IComponent : ILogic
    {
        public virtual void Enter(Entity actor)
        {

        }

        public virtual void Enter()
        {
            
        }

        public virtual void Exit()
        {

        }

        public virtual void Init()
        {

        }

        public virtual void Tick(float deltaTime)
        {

        }
    }
}

