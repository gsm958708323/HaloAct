
using UnityEngine;

namespace Combat
{
    public struct ColliderData
    {
        public ColliderType Type;

        // Sphere
        public float Radius;

        // Box (OBB 半尺寸)
        public Vector3 HalfExtents;

        // 空间变换
        public Vector3 Center;
        public Quaternion Rotation;

        public static ColliderData CreateSphere(Vector3 center, float radius)
        {
            return new ColliderData
            {
                Type = ColliderType.Sphere,
                Center = center,
                Radius = radius,
                Rotation = Quaternion.identity,
            };
        }

        public static ColliderData CreateBox(Vector3 center, Vector3 halfExtents,
            Quaternion rotation)
        {
            return new ColliderData
            {
                Type = ColliderType.Box,
                Center = center,
                HalfExtents = halfExtents,
                Rotation = rotation,
            };
        }
    }

    public enum ColliderType
    {
        Box,
        Sphere
    }

    public struct SweepInput
    {
        public Vector3 Origin;
        public Vector3 End;
        public float Radius;
    }

    public struct SweepResult
    {
        public bool Hit;
        public float T;            // [0,1] 命中参数
        public Vector3 HitPoint;
        public Entity HitEntity;
    }
}