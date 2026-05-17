namespace Combat
{
    public struct Modifier
    {
        public ModifierTarget Direction;
        public ModifierOp Op;
        public DamageType AffectedType;
        public float Value;
        public Modifier(ModifierTarget dir, ModifierOp op, DamageType affectedType, float value)
        {
            Direction = dir;
            Op = op;
            AffectedType = affectedType;
            Value = value;
        }
    }

    /// <summary>
    /// ModifierCache 中的聚合结果。
    /// 一个 (Direction, DamageType) 对应一个 AggregatedModifier。
    /// </summary>
    public class AggregatedModifier
    {
        public float SumPercentAdd;
        public float SumFlatAdd;
        public float ProductPercentMul;

        public AggregatedModifier()
        {
            Reset();
        }

        public void Reset()
        {
            SumPercentAdd = 0f;
            SumFlatAdd = 0f;
            ProductPercentMul = 1f;
        }

        /// <summary>
        /// 公式: (baseValue * (1 + SumPercentAdd) + SumFlatAdd) * ProductPercentMul
        /// </summary>
        public float Apply(float baseValue)
        {
            return (baseValue * (1f + SumPercentAdd) + SumFlatAdd) * ProductPercentMul;
        }
    }

     /// <summary>
    /// ModifierCache 的 Key：方向 + 伤害类型。
    /// </summary>
    public struct ModifierKey : System.IEquatable<ModifierKey>
    {
        public ModifierTarget Direction;
        public DamageType DamageType;

        public ModifierKey(ModifierTarget dir, DamageType type)
        {
            Direction = dir;
            DamageType = type;
        }

        public bool Equals(ModifierKey other)
        {
            return Direction == other.Direction && DamageType == other.DamageType;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Direction, DamageType);
        }

        public override bool Equals(object obj)
        {
            return obj is ModifierKey other && Equals(other);
        }
    }

    public enum DamageType
    {
        None,       // 全类型通配
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison,
    }

    public enum EffectType
    {
        Damage,
        Heal,
        ApplyBuff,
        RemoveBuff,
        SpawnAOE,
        SpawnBullet,
        Dispel,
    }

    public enum ModifierOp
    {
        PercentAdd,     // 加法叠加百分比
        FlatAdd,        // 固定值加减
        PercentMul,     // 乘法叠加百分比
        Override,       // 强制覆盖
    }

    public enum ModifierTarget
    {
        Outgoing,       // 输出（攻击方加成）
        Incoming,       // 输入（防御方减免）
    }

    public enum StackMode
    {
        None,           // 不叠层，只刷新持续时间
        Shared,         // 所有来源共享叠层
        BySource,       // 不同来源独立叠层
        Independent,    // 每次施加都创建独立实例
    }
}