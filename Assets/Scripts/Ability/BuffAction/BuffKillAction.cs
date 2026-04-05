namespace Ability
{
    public class BuffKillAction
    {
        protected EffectObj buff;
        protected DamageInfo damage;

        public void Execute(EffectObj buff, DamageInfo damage)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.Buff);
            this.buff = buff;
            this.damage = damage;
            OnExecute();
        }

        protected virtual void OnExecute()
        {
        }
    }
}
