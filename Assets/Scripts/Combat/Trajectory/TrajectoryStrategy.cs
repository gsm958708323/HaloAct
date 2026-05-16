using UnityEngine;

namespace Combat
{
    /// <summary>
    /// 直线
    /// </summary>
    public class StraightTrajectory : ITrajectoryStrategy
    {
        public void Evaluate(TrajectoryComponent t, TransformComponent tr,
            float dt, out Vector3 newPos, out Vector3 newFwd)
        {
            newFwd = tr.Forward;
            float move = t.Speed * dt;
            newPos = tr.Position + newFwd * move;

            t.ElapsedTime += dt;
            t.TotalDistance += move;
        }
    }

    public class ParabolaTrajectory : ITrajectoryStrategy
    {
        public void Evaluate(
            TrajectoryComponent t, TransformComponent tr,
            float dt, out Vector3 newPos, out Vector3 newFwd)
        {
            t.ElapsedTime += dt;
            float elapsed = t.ElapsedTime;

            // ParamVec0 = initialVelocity, ParamFloat0 = gravity
            Vector3 v0 = t.ParamVec0;
            Vector3 gravity = new Vector3(0f, -t.ParamFloat0, 0f);

            newPos = t.Origin + v0 * elapsed
                + 0.5f * gravity * elapsed * elapsed;

            // 朝向 = 当前速度方向
            Vector3 velocity = v0 + gravity * elapsed;
            newFwd = velocity.sqrMagnitude > 1e-6f
                ? velocity.normalized
                : tr.Forward;

            t.TotalDistance += (newPos - tr.Position).magnitude;
        }
    }

    public interface ITrajectoryStrategy
    {
        // 根据轨迹参数和时间，计算当前位置和朝向
        void Evaluate(
            TrajectoryComponent trajectory,
            TransformComponent transform,
            float deltaTime,
            out Vector3 newPosition,
            out Vector3 newForward);
    }
}

