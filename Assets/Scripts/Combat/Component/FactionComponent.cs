using UnityEngine;

namespace Combat
{
    public class FactionComponent : IComponent
    {
        /// <summary>
        /// 阵营id
        /// </summary>
        public int FactionId;
    }

    public static class FactionHelper
    {
        public static bool IsHostile(World world, Entity a, Entity b)
        {
            var fa = world.GetComponent<FactionComponent>(a);
            var fb = world.GetComponent<FactionComponent>(b);
            if (fa == null || fb == null) return false;
            return fa.FactionId != fb.FactionId;
        }
    }
}
