using UnityEngine;

namespace Combat
{
    [CreateAssetMenu]
    public class BulletConfig : ScriptableObject
    {
        public int BulletId;
        public float Speed = 10;
        public float LifeTime = 3;
        public float CollisionRadius = 0.3f;
        /// <summary>
        /// 移动轨迹
        /// </summary>
        public TrajectoryType Trajectory;
        public float[] TrajectoryParams;
        /// <summary>
        /// 击中类型
        /// </summary>
        public HitMode HitMode;
        public int MaxHitCount = 1;
        public float BounceRange;
        public bool FollowSource;
        public EffectGroup[] PayloadGroups;
    }

    public enum TrajectoryType
    {
        Straight,      // 直线
        Parabola,      // 抛物线
        Bezier,        // 贝塞尔曲线
        Tracking,      // 追踪目标
        Boomerang,     // 回旋镖（去+返）
    }

    public enum HitMode
    {
        Single,        // 命中一个即销毁
        Penetrate,     // 穿透，继续飞行，命中多个
        Bounce,        // 弹射到下一个目标
        AreaOnHit,     // 命中时不直接造成效果，改为生成 AOE（由 Payload 配置）
    }
}