using UnityEngine;

namespace Combat
{
    [CreateAssetMenu]
    public class EffectConfig : ScriptableObject
    {
        public EffectType Type;
        public TargetRule Target;
        public float[] Params;

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