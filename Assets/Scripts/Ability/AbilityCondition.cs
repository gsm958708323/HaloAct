namespace Ability
{
    /// <summary>
    /// 切换下一个行为的条件
    /// </summary>
    public abstract class AbilityCondition
    {
        public virtual bool Check(AbilityBehaviorTree tree) { return true; }
    }
}