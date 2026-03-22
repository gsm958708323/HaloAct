using System.Collections;
using System.Collections.Generic;

namespace Ability
{
    public class BulletAction
    {
        protected BulletDataComp bullet;
        protected Entity target;

        public void Execute(BulletDataComp bullet, Entity target)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.Bullet);
            this.bullet = bullet;
            this.target = target;
            OnExecute();
        }

        protected virtual void OnExecute()
        {
        }
    }
}
