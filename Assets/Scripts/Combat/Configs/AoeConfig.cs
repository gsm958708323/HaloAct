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
        /// <summary>
        /// 进入区域效果
        /// </summary>
        public EffectConfig[] OnEnterEffects;
        /// <summary>
        /// 持续每帧的效果
        /// </summary>
        public EffectConfig[] OnTickEffects;
        /// <summary>
        /// 退出区域效果
        /// </summary>
        public EffectConfig[] OnExitEffects;
    }

    public class ShapeType
    {
    }
}