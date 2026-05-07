using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public class EffectRequest
    {
        // --- 身份 ---
        public EffectType Type;         // Damage, Heal, ApplyBuff, SpawnAOE, SpawnBullet

        // --- 来源 ---
        public Entity Source;           // 效果发起者（施法者）
        public Entity Instigator;      // 效果直接载体（子弹/AOE Entity）

        // --- 目标 ---
        public Entity Target;          // 受影响的实体

        // --- 数值（可被 Pipeline 阶段修改） ---
        public float Value;            // 伤害/治疗量
        public DamageType DamageType;  // 物理/火/冰/毒 ...

        // --- 扩展参数 ---
        public int BuffConfigId;       // Type=ApplyBuff 时使用
        public int AOEConfigId;        // Type=SpawnAOE 时使用
        public int BulletConfigId;     // Type=SpawnBullet 时使用

        // --- 管线控制标记 ---
        public bool Cancelled;         // 被免疫等阶段标记为取消
        public bool Absorbed;          // 被护盾完全吸收

    }

    public enum DamageType
    {
    }

    public interface IEffectProcessor
    {
        string Name { get; }
        void Process(EffectRequest request, World world);
    }
}
