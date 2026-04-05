using System;
using UnityEngine;

namespace Ability
{
    [Serializable]
    public struct AoeMoveInfo
    {
        public Vector3 SpawnOffset;
        public Vector3 Velocity;

        public static AoeMoveInfo CreateDefault()
        {
            return new AoeMoveInfo
            {
                SpawnOffset = Vector3.zero,
                Velocity = Vector3.zero,
            };
        }
    }
}
