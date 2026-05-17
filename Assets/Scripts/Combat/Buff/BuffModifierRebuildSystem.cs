namespace Combat
{
    /// <summary>
    /// 当 ModifierCacheComponent.Dirty == true 时，
    /// 遍历该角色所有 Buff 的 Modifier，重新聚合缓存。
    /// </summary>
    public class BuffModifierRebuildSystem : ISystem
    {
        public override void Tick(float delteTime)
        {
            var owners = world.Query<ModifierCacheComponent,
                                      BuffContainerComponent>();

            foreach (var owner in owners)
            {
                var cache = world.GetComponent<ModifierCacheComponent>(owner);
                if (!cache.Dirty) continue;

                foreach (var item in cache.Cache.Values)
                {
                    item.Reset();
                }

                var container = world.GetComponent<BuffContainerComponent>(owner);
                for (int i = 0; i < container.Instances.Count; i++)
                {
                    var buff = container.Instances[i];
                    if (buff.Modifiers == null || buff.Modifiers.Count == 0) continue;

                    for (int m = 0; m < buff.Modifiers.Count; m++)
                    {
                        var mod = buff.Modifiers[m];
                        var key = new ModifierKey(mod.Direction, mod.AffectedType);
                        if (!cache.Cache.TryGetValue(key, out var agg))
                        {
                            agg = new AggregatedModifier();
                            cache.Cache[key] = agg;
                        }

                        switch (mod.Op)
                        {
                            case ModifierOp.PercentAdd:
                                agg.SumPercentAdd += mod.Value;
                                break;
                            case ModifierOp.FlatAdd:
                                agg.SumFlatAdd += mod.Value;
                                break;
                            case ModifierOp.PercentMul:
                                agg.ProductPercentMul *= (1f + mod.Value);
                                break;
                            case ModifierOp.Override:
                                // Override 特殊处理：直接设置，后续 Apply 时检测
                                agg.SumPercentAdd = 0f;
                                agg.SumFlatAdd = mod.Value;
                                agg.ProductPercentMul = 0f; // 标记为 Override
                                break;
                        }
                    }
                }

                cache.Dirty = false;
            }
        }
    }
}