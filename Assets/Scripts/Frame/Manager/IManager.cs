using System;

namespace Frame
{
    public abstract class IManager : ILogic
    {
        public virtual int Priority { get { return 0; } }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void Init()
        {
        }

        public virtual void Tick(int frame)
        {
        }

        public virtual void Update(float deltaTime)
        {

        }
    }
}
