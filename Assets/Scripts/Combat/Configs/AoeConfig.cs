using UnityEngine;

namespace Combat
{
    [CreateAssetMenu]
    public class AoeConfig : ScriptableObject
    {
        public int AoeId;
        public ShapeType Shape;
        public float[] ShapeParams;
        public float Duration;
        public float TickInterval;
        public EffectGroup[] PayloadGroups { get; internal set; }
        public bool FollowSource { get; internal set; }
    }

    public enum ShapeType
    {
        Circle,
        Sector,
        Rectangle
    }
}