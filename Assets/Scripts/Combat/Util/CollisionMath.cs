using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public static class CollisionMath
    {
        // ==============================================================
        //  静态重叠检测
        // ==============================================================

        public static bool Overlap(in ColliderData a, in ColliderData b)
        {
            if (a.Type == ColliderType.Sphere && b.Type == ColliderType.Sphere)
                return SphereSphere(a, b);
            if (a.Type == ColliderType.Sphere && b.Type == ColliderType.Box)
                return SphereBox(a, b);
            if (a.Type == ColliderType.Box && b.Type == ColliderType.Sphere)
                return SphereBox(b, a);
            if (a.Type == ColliderType.Box && b.Type == ColliderType.Box)
                return BoxBox(a, b);

            return false;
        }

        // ==============================================================
        //  Sphere vs Sphere
        // ==============================================================

        public static bool SphereSphere(in ColliderData a, in ColliderData b)
        {
            float r = a.Radius + b.Radius;
            return (a.Center - b.Center).sqrMagnitude <= r * r;
        }

        // ==============================================================
        //  Sphere vs OBB
        // ==============================================================

        public static bool SphereBox(in ColliderData sphere, in ColliderData box)
        {
            Vector3 localCenter = InverseTransformPoint(
                sphere.Center, box.Center, box.Rotation);

            Vector3 closest;
            closest.x = Mathf.Clamp(localCenter.x,
                -box.HalfExtents.x, box.HalfExtents.x);
            closest.y = Mathf.Clamp(localCenter.y,
                -box.HalfExtents.y, box.HalfExtents.y);
            closest.z = Mathf.Clamp(localCenter.z,
                -box.HalfExtents.z, box.HalfExtents.z);

            return (localCenter - closest).sqrMagnitude
                <= sphere.Radius * sphere.Radius;
        }

        // ==============================================================
        //  OBB vs OBB  (SAT — Separating Axis Theorem)
        // ==============================================================

        public static bool BoxBox(in ColliderData a, in ColliderData b)
        {
            // 提取旋转轴
            GetAxes(a.Rotation, out Vector3 aR, out Vector3 aU, out Vector3 aF);
            GetAxes(b.Rotation, out Vector3 bR, out Vector3 bU, out Vector3 bF);

            Vector3[] axesA = { aR, aU, aF };
            Vector3[] axesB = { bR, bU, bF };
            float[] halfA = { a.HalfExtents.x, a.HalfExtents.y, a.HalfExtents.z };
            float[] halfB = { b.HalfExtents.x, b.HalfExtents.y, b.HalfExtents.z };

            Vector3 d = b.Center - a.Center;

            // 检测 A 的 3 个轴
            for (int i = 0; i < 3; i++)
                if (IsSeparated(axesA[i], axesA, halfA, axesB, halfB, d))
                    return false;

            // 检测 B 的 3 个轴
            for (int i = 0; i < 3; i++)
                if (IsSeparated(axesB[i], axesA, halfA, axesB, halfB, d))
                    return false;

            // 检测 9 条叉积轴
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 cross = Vector3.Cross(axesA[i], axesB[j]);
                    if (cross.sqrMagnitude < 1e-6f) continue;
                    if (IsSeparated(cross.normalized, axesA, halfA, axesB, halfB, d))
                        return false;
                }
            }

            return true;
        }

        private static bool IsSeparated(Vector3 axis,
            Vector3[] axesA, float[] halfA,
            Vector3[] axesB, float[] halfB,
            Vector3 d)
        {
            float rA = 0f, rB = 0f;
            for (int i = 0; i < 3; i++)
            {
                rA += halfA[i] * Mathf.Abs(Vector3.Dot(axesA[i], axis));
                rB += halfB[i] * Mathf.Abs(Vector3.Dot(axesB[i], axis));
            }

            float dist = Mathf.Abs(Vector3.Dot(d, axis));
            return dist > rA + rB;
        }

        // ==============================================================
        //  SweptSphere（体积扫描）
        // ==============================================================

        public static SweepResult Sweep(in SweepInput sweep, in ColliderData target)
        {
            if (target.Type == ColliderType.Sphere)
                return SweptSphereSphere(sweep, target);
            if (target.Type == ColliderType.Box)
                return SweptSphereBox(sweep, target);

            return default;
        }

        // --- SweptSphere vs Sphere ---

        public static SweepResult SweptSphereSphere(
            in SweepInput sweep, in ColliderData target)
        {
            Vector3 dir = sweep.End - sweep.Origin;
            Vector3 m = sweep.Origin - target.Center;
            float combinedR = sweep.Radius + target.Radius;

            float a = Vector3.Dot(dir, dir);

            // 零位移
            if (a < 1e-8f)
            {
                bool inside = m.sqrMagnitude <= combinedR * combinedR;
                return new SweepResult
                {
                    Hit = inside,
                    T = 0f,
                    HitPoint = sweep.Origin
                };
            }

            float b = 2f * Vector3.Dot(m, dir);
            float c = Vector3.Dot(m, m) - combinedR * combinedR;

            // 起点已在球内
            if (c <= 0f)
                return new SweepResult
                {
                    Hit = true,
                    T = 0f,
                    HitPoint = sweep.Origin
                };

            float disc = b * b - 4f * a * c;
            if (disc < 0f)
                return default;

            float sqrtDisc = Mathf.Sqrt(disc);
            float t = (-b - sqrtDisc) / (2f * a);

            if (t >= 0f && t <= 1f)
            {
                return new SweepResult
                {
                    Hit = true,
                    T = t,
                    HitPoint = sweep.Origin + t * dir
                };
            }

            return default;
        }

        // --- SweptSphere vs OBB (膨胀 OBB + Slab Method) ---

        public static SweepResult SweptSphereBox(
            in SweepInput sweep, in ColliderData box)
        {
            Quaternion invRot = Quaternion.Inverse(box.Rotation);
            Vector3 localOrigin = invRot * (sweep.Origin - box.Center);
            Vector3 localEnd = invRot * (sweep.End - box.Center);
            Vector3 localDir = localEnd - localOrigin;

            // 膨胀
            Vector3 expandedHalf = box.HalfExtents +
                new Vector3(sweep.Radius, sweep.Radius, sweep.Radius);

            float tMin = 0f;
            float tMax = 1f;

            for (int i = 0; i < 3; i++)
            {
                float origin_i = localOrigin[i];
                float dir_i = localDir[i];
                float half_i = expandedHalf[i];

                if (Mathf.Abs(dir_i) < 1e-8f)
                {
                    if (origin_i < -half_i || origin_i > half_i)
                        return default;
                }
                else
                {
                    float invD = 1f / dir_i;
                    float t1 = (-half_i - origin_i) * invD;
                    float t2 = (half_i - origin_i) * invD;

                    if (t1 > t2) (t1, t2) = (t2, t1);

                    tMin = Mathf.Max(tMin, t1);
                    tMax = Mathf.Min(tMax, t2);

                    if (tMin > tMax)
                        return default;
                }
            }

            // 起点在膨胀 OBB 内
            if (tMin <= 0f && tMax >= 0f)
            {
                return new SweepResult
                {
                    Hit = true,
                    T = 0f,
                    HitPoint = sweep.Origin
                };
            }

            if (tMin >= 0f && tMin <= 1f)
            {
                Vector3 localHit = localOrigin + tMin * localDir;
                Vector3 worldHit = box.Rotation * localHit + box.Center;
                return new SweepResult
                {
                    Hit = true,
                    T = tMin,
                    HitPoint = worldHit
                };
            }

            return default;
        }

        // ==============================================================
        //  批量扫描
        // ==============================================================

        /// <summary>
        /// 一个 SweepInput 对多个目标做检测，结果按 T 排序。
        /// results 由调用方提供（复用），内部 Clear。
        /// </summary>
        public static void SweepAll(
            in SweepInput sweep,
            IReadOnlyList<ColliderData> targets,
            IReadOnlyList<Entity> entities,
            List<SweepResult> results)
        {
            results.Clear();

            for (int i = 0; i < targets.Count; i++)
            {
                var result = Sweep(sweep, targets[i]);
                if (result.Hit)
                {
                    result.HitEntity = entities[i];
                    results.Add(result);
                }
            }

            results.Sort(CompareSweepByT);
        }

        private static readonly System.Comparison<SweepResult> CompareSweepByT =
            (a, b) => a.T.CompareTo(b.T);

        // ==============================================================
        //  扇形检测辅助
        // ==============================================================

        /// <summary>
        /// 检查 targetPos 是否在以 center 为圆心、forward 为正方向的扇形内。
        /// halfAngle 为半角（度）。
        /// 不含距离检查 —— 距离由 Sphere Overlap 保证。
        /// </summary>
        public static bool IsInSector(
            Vector3 center, Vector3 forward,
            Vector3 targetPos, float halfAngle)
        {
            Vector3 toTarget = targetPos - center;
            toTarget.y = 0f;   // 投影到水平面
            if (toTarget.sqrMagnitude < 1e-6f) return true; // 重叠

            float angle = Vector3.Angle(
                new Vector3(forward.x, 0f, forward.z), toTarget);
            return angle <= halfAngle;
        }

        // ==============================================================
        //  内部工具
        // ==============================================================

        private static Vector3 InverseTransformPoint(
            Vector3 worldPoint, Vector3 localOrigin, Quaternion localRotation)
        {
            return Quaternion.Inverse(localRotation) * (worldPoint - localOrigin);
        }

        private static void GetAxes(Quaternion rotation,
            out Vector3 right, out Vector3 up, out Vector3 forward)
        {
            right = rotation * Vector3.right;
            up = rotation * Vector3.up;
            forward = rotation * Vector3.forward;
        }

        // ==============================================================
        //  调试绘制
        // ==============================================================

#if UNITY_EDITOR
        public static void DebugDrawCollider(in ColliderData data, Color color,
            float duration = 0f)
        {
            switch (data.Type)
            {
                case ColliderType.Sphere:
                    DebugDrawWireSphere(data.Center, data.Radius, color, duration);
                    break;
                case ColliderType.Box:
                    DebugDrawWireBox(data.Center, data.HalfExtents,
                        data.Rotation, color, duration);
                    break;
            }
        }

        public static void DebugDrawSweep(in SweepInput sweep, Color color,
            float duration = 0f)
        {
            Debug.DrawLine(sweep.Origin, sweep.End, color, duration);
            DebugDrawWireSphere(sweep.Origin, sweep.Radius, color, duration);
            DebugDrawWireSphere(sweep.End, sweep.Radius, color, duration);
        }

        private static void DebugDrawWireSphere(Vector3 center, float radius,
            Color color, float duration)
        {
            int segments = 16;
            float step = 360f / segments;

            DrawCircle(center, Vector3.up, Vector3.right,
                radius, segments, step, color, duration);
            DrawCircle(center, Vector3.right, Vector3.up,
                radius, segments, step, color, duration);
            DrawCircle(center, Vector3.forward, Vector3.up,
                radius, segments, step, color, duration);
        }

        private static void DrawCircle(Vector3 center, Vector3 axis, Vector3 up,
            float radius, int segments, float step, Color color, float duration)
        {
            Quaternion rot = Quaternion.LookRotation(axis, up);
            Vector3 prev = center + rot * (Vector3.up * radius);

            for (int i = 1; i <= segments; i++)
            {
                float angle = step * i * Mathf.Deg2Rad;
                Vector3 next = center + rot *
                    new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * radius;
                Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
        }

        private static void DebugDrawWireBox(Vector3 center, Vector3 halfExtents,
            Quaternion rotation, Color color, float duration)
        {
            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                Vector3 local = new Vector3(
                    (i & 1) == 0 ? -halfExtents.x : halfExtents.x,
                    (i & 2) == 0 ? -halfExtents.y : halfExtents.y,
                    (i & 4) == 0 ? -halfExtents.z : halfExtents.z
                );
                corners[i] = center + rotation * local;
            }

            // 底面
            Debug.DrawLine(corners[0], corners[1], color, duration);
            Debug.DrawLine(corners[1], corners[3], color, duration);
            Debug.DrawLine(corners[3], corners[2], color, duration);
            Debug.DrawLine(corners[2], corners[0], color, duration);
            // 顶面
            Debug.DrawLine(corners[4], corners[5], color, duration);
            Debug.DrawLine(corners[5], corners[7], color, duration);
            Debug.DrawLine(corners[7], corners[6], color, duration);
            Debug.DrawLine(corners[6], corners[4], color, duration);
            // 竖边
            Debug.DrawLine(corners[0], corners[4], color, duration);
            Debug.DrawLine(corners[1], corners[5], color, duration);
            Debug.DrawLine(corners[2], corners[6], color, duration);
            Debug.DrawLine(corners[3], corners[7], color, duration);
        }
#endif
    }
}