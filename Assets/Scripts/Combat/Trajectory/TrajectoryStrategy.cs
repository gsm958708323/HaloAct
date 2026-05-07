using UnityEngine;

namespace Combat
{
    /// <summary>
    /// 直线
    /// </summary>
    // public class StraightTrajectory : ITrajectoryStrategy
    // {
    //     public void Evaluate(TrajectoryComponent comp, float dt, out Vector3 pos, out Vector3 forward)
    //     {
    //         forward = comp.Forward; 
    //     }
    // }

    public interface ITrajectoryStrategy
    {
        // 根据轨迹参数和时间，计算当前位置和朝向
        void Evaluate(
            TrajectoryComponent t,
            float dt,
            out Vector3 pos,
            out Vector3 forward
        );
    }
}

