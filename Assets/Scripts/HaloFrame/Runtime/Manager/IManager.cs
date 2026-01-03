using System;

namespace HaloFrame
{
    public abstract class IManager
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

        public virtual void Destroy()
        {
        }

        /// <summary>
        /// 相同帧间隔更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Tick(float deltaTime)
        {
        }

        /// <summary>
        /// 渲染间隔更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(float deltaTime)
        {

        }
    }
}
