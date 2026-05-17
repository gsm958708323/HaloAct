using System.Collections.Generic;

namespace Combat
{
    public class ModifierCacheComponent : IComponent
    {
        public Dictionary<ModifierKey, AggregatedModifier> Cache;
        public bool Dirty { get; internal set; }
        public ModifierCacheComponent()
        {
            Cache = new Dictionary<ModifierKey, AggregatedModifier>(8);
        }
        /// <summary>
        /// 获取指定方向和伤害类型的聚合修改器。
        /// 如果不存在则返回 null。
        /// </summary>
        public AggregatedModifier Get(ModifierTarget direction, DamageType damageType)
        {
            var key = new ModifierKey(direction, damageType);
            return Cache.TryGetValue(key, out var agg) ? agg : null;
        }
    }
}