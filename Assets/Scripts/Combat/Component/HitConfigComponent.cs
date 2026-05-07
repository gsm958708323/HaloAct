namespace Combat
{
    /// <summary>
    /// 碰撞体描述"物理形状"，HitConfig 描述"命中后的行为规则"
    /// </summary>
    public class HitConfigComponent : IComponent
    {
        public HitMode Mode;          // Single, Penetrate, Bounce
        public int MaxHitCount;       // 最大命中次数
        public float BounceRange;     // 弹射搜索范围
    }
}
