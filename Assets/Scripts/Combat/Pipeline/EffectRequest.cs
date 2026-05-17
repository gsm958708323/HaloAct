using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class EffectRequest
    {
                // 效果类型
        public EffectType Type;

        // 来源
        public Entity Source;
        public Entity Instigator;

        // 目标
        public Entity Target;

        // 数值
        public float Value;
        public DamageType DamageType;

        // 扩展引用
        public int ReferenceId;     // BuffConfigId / AOEConfigId / BulletConfigId

        // 空间信息（命中点、方向）
        public Vector3 HitPoint;
        public Vector3 Direction;

        // 管线控制
        public bool Cancelled;
        public bool Absorbed;
    }

    public interface IEffectProcessor
    {
        string Name { get; }
        void Process(EffectRequest request, World world);
    }
}
