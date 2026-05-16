using UnityEngine;

namespace Combat
{
    [CreateAssetMenu]
    public class EffectConfig : ScriptableObject
    {
        public EffectType Type;
        public TargetRule Target;
        public float[] Params;

        public float Value { get; internal set; }
        public DamageType DamageType { get; internal set; }
        public int ReferenceId { get; internal set; }
    }
    public enum EffectType
    {
        SpawnAOE,
        SpawnBullet
    }

    public enum TargetRule
    {
    }
}