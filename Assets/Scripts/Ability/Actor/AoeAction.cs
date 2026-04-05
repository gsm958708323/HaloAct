namespace Ability
{
    public class AoeAction
    {
        protected AoeDataComp aoe;
        protected Entity target;

        public void Execute(AoeDataComp aoe, Entity target)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.Bullet);
            this.aoe = aoe;
            this.target = target;
            OnExecute();
        }

        protected virtual void OnExecute()
        {
        }
    }
}
