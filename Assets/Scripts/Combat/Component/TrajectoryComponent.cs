using UnityEngine;

namespace Combat
{
    public class TrajectoryComponent : IComponent
    {
        public TrajectoryType Type;

        // --- 共用参数 ---
        public float Speed;

        // --- 轨迹运行时状态 ---
        public float ElapsedTime;      // 已飞行时间
        public float TotalDistance;     // 已飞行总距离
        public Vector3 Origin;         // 发射点

        // --- 类型专用参数（由工厂根据 Type 填充）---
        public Vector3 ParamVec0;      // 用途随 Type 变化
        public Vector3 ParamVec1;
        public float ParamFloat0;
        public float ParamFloat1;
        public Entity TargetEntity;    // Tracking 类型专用

        public Vector3 Forward { get; internal set; }
        public Vector3 PreviousPosition { get; internal set; }
        public Vector3 TargetPosition { get; internal set; }
    }
}
