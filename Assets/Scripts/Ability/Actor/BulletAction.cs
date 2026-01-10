using System.Collections;
using System.Collections.Generic;

namespace Ability
{
    public class BulletAction
    {
        public void Execute()
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.Bullet);
            OnExecute();
        }

        protected virtual void OnExecute()
        {
        }
    }
}
