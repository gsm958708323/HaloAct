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

        public void Destroy()
        {
            
        }

        public virtual void Tick(float deltaTime)
        {

        }
    }
}

