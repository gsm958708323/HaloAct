using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// 用来计算过滤目标
    /// </summary>
    public class TargetCacheComponent : IComponent
    {
        /// <summary>
        /// 上一次检测命中的目标i
        /// </summary>
        public HashSet<Entity> Previous;
        /// <summary>
        /// 当前检测命中的目标
        /// </summary>
        public HashSet<Entity> Current;
        /// <summary>
        /// 总交互次数
        /// </summary>
        public int TotalCount;

        // ---- 集合操作（在 System 中调用）----
        // Entered = Current - Previous
        // Stayed  = Current ∩ Previous  
        // Exited  = Previous - Current
        // 交换: (Previous, Current) = (Current, Previous); Current.Clear();
    }
}
