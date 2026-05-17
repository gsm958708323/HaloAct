using System;
using UnityEngine;

namespace Combat
{
    public class BulletMoveSystem : ISystem
    {
        public override void Tick(float delteTime)
        {
            var bullets = world.Query<TrajectoryComponent, TransformComponent>();

            foreach (var entity in bullets)
            {
                var trajectory = world.GetComponent<TrajectoryComponent>(entity);
                var transform = world.GetComponent<TransformComponent>(entity);

                // 保存上一帧位置（Sweep 碰撞用）
                trajectory.PreviousPosition = transform.Position;

                // 追踪类型：更新目标位置
                UpdateTrackingTarget(entity, trajectory);

                var strategy = TrajectoryStrategies.Get(trajectory.Type);
                strategy.Evaluate(trajectory, transform, delteTime,
                out Vector3 newPos, out Vector3 newFwd);

                transform.Position = newPos;
                transform.Forward = newFwd;
            }
        }

        private void UpdateTrackingTarget(Entity entity, TrajectoryComponent trajectory)
        {
            switch (trajectory.Type)
            {
                case TrajectoryType.Tracking:
                    if (!trajectory.TargetEntity.IsNull
                        && world.IsAlive(trajectory.TargetEntity))
                    {
                        var targetTr = world.GetComponent<TransformComponent>(
                            trajectory.TargetEntity);
                        if (targetTr != null)
                            trajectory.TargetPosition = targetTr.Position;
                    }
                    break;
            }
        }
    }
}