using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Bullet 碰撞检测。使用 SweptSphere 做连续碰撞。
    /// 命中后根据 HitMode 决定后续行为。
    /// </summary>
    public class CollisionSystem : ISystem
    {
        private readonly EffectRequestBuffer effectBuffer;
        private readonly List<SweepResult> sweepResults = new(16);

        public CollisionSystem(EffectRequestBuffer effectBuffer)
        {
            this.effectBuffer = effectBuffer;
        }

        public override void Tick(float deltaTime)
        {
            var bullets = world.Query
            <HitConfigComponent, TrajectoryComponent, TransformComponent>();

            foreach (var bulletEntity in bullets)
            {
                if (world.HasComponent<DestroyTagComponent>(bulletEntity)) continue;

                var hitConfig = world.GetComponent<HitConfigComponent>(bulletEntity);
                var trajectory = world.GetComponent<TrajectoryComponent>(bulletEntity);
                var bulletTransform = world.GetComponent<TransformComponent>(
                    bulletEntity);
                var sourceInfo = world.GetComponent<SourceInfoComponent>(bulletEntity);
                var collider = world.GetComponent<ColliderComponent>(bulletEntity);

                if (sourceInfo == null || collider == null) continue;

                // 构造 SweepInput
                var sweep = new SweepInput
                {
                    Origin = trajectory.PreviousPosition,
                    End = bulletTransform.Position,
                    Radius = collider.Data.Radius,
                };

                // 零位移跳过
                if ((sweep.End - sweep.Origin).sqrMagnitude < 1e-8f) continue;

                // 收集所有候选目标
                CollectAndSweep(bulletEntity, sourceInfo, sweep);

                // 处理命中结果
                ProcessHits(bulletEntity, hitConfig, sourceInfo, bulletTransform);
            }
        }

        private void CollectAndSweep(Entity bulletEntity,
            SourceInfoComponent sourceInfo, SweepInput sweep)
        {
            sweepResults.Clear();

            var targets = world.Query<ColliderComponent, TransformComponent,
                                       FactionComponent>();

            foreach (var candidate in targets)
            {
                if (candidate == bulletEntity) continue;

                // 不能打自己的主人
                if (candidate == sourceInfo.Source) continue;

                // 阵营过滤
                if (!FactionHelper.IsHostile(world, sourceInfo.Source, candidate))
                    continue;

                // 已命中过的跳过
                var memory = world.GetComponent<TargetMemoryComponent>(bulletEntity);
                if (memory != null && memory.Previous.Contains(candidate))
                    continue;

                var targetCollider = world.GetComponent<ColliderComponent>(candidate);
                var targetTransform = world.GetComponent<TransformComponent>(
                    candidate);

                targetCollider.Data.Center = targetTransform.Position;

                var result = CollisionMath.Sweep(sweep, targetCollider.Data);
                if (result.Hit)
                {
                    result.HitEntity = candidate;
                    sweepResults.Add(result);
                }
            }

            // 按距离排序
            sweepResults.Sort((a, b) => a.T.CompareTo(b.T));
        }

        private void ProcessHits(Entity bulletEntity,
            HitConfigComponent hitConfig,
            SourceInfoComponent sourceInfo,
            TransformComponent bulletTransform)
        {
            for (int i = 0; i < sweepResults.Count; i++)
            {
                if (world.HasComponent<DestroyTagComponent>(bulletEntity)) break;

                var hit = sweepResults[i];
                if (!world.IsAlive(hit.HitEntity)) continue;

                // 记录命中
                var memory = world.GetComponent<TargetMemoryComponent>(bulletEntity);
                if (memory != null)
                {
                    memory.Previous.Add(hit.HitEntity);
                    memory.TotalHitCount++;
                }

                // 提交 OnHit 效果
                var payload = world.GetComponent<EffectPayloadComponent>(bulletEntity);
                if (payload != null)
                {
                    var hitEffects = PayloadHelper.GetEffects(
                        payload, PayloadTrigger.OnHit);
                    EffectSubmitHelper.Submit(effectBuffer,
                        sourceInfo.Source, bulletEntity, hit.HitEntity,
                        hitEffects, hit.HitPoint, bulletTransform.Forward);
                }

                // 根据 HitMode 决定后续行为
                switch (hitConfig.Mode)
                {
                    case HitMode.Single:
                        SubmitExpireAndDestroy(bulletEntity, sourceInfo, hit.HitPoint);
                        return;

                    case HitMode.Penetrate:
                        if (memory != null
                            && memory.TotalHitCount >= hitConfig.MaxHitCount)
                        {
                            SubmitExpireAndDestroy(
                                bulletEntity, sourceInfo, hit.HitPoint);
                            return;
                        }
                        break;

                    case HitMode.Bounce:
                        if (memory != null
                            && memory.TotalHitCount >= hitConfig.MaxHitCount)
                        {
                            SubmitExpireAndDestroy(
                                bulletEntity, sourceInfo, hit.HitPoint);
                            return;
                        }
                        BounceToNext(bulletEntity, hitConfig, hit);
                        return; // 弹射后本帧不再检测
                }
            }
        }

        private void BounceToNext(Entity bulletEntity,
            HitConfigComponent hitConfig, SweepResult lastHit)
        {
            var transform = world.GetComponent<TransformComponent>(bulletEntity);
            var trajectory = world.GetComponent<TrajectoryComponent>(bulletEntity);
            var sourceInfo = world.GetComponent<SourceInfoComponent>(bulletEntity);
            var memory = world.GetComponent<TargetMemoryComponent>(bulletEntity);

            // 搜索范围内最近的未命中敌人
            Entity nextTarget = Entity.Null;
            float minDist = float.MaxValue;

            var candidates = world.Query
            <ColliderComponent, TransformComponent, FactionComponent>();

            foreach (var candidate in candidates)
            {
                if (candidate == bulletEntity) continue;
                if (candidate == sourceInfo.Source) continue;
                if (candidate == lastHit.HitEntity) continue;
                if (memory != null && memory.Previous.Contains(candidate)) continue;
                if (!FactionHelper.IsHostile(world, sourceInfo.Source, candidate))
                    continue;

                var candidateTransform = world.GetComponent<TransformComponent>(
                    candidate);
                float dist = Vector3.Distance(
                    lastHit.HitPoint, candidateTransform.Position);

                if (dist <= hitConfig.BounceRange && dist < minDist)
                {
                    minDist = dist;
                    nextTarget = candidate;
                }
            }

            if (nextTarget.IsNull)
            {
                SubmitExpireAndDestroy(bulletEntity, sourceInfo, lastHit.HitPoint);
                return;
            }

            // 重定向子弹
            var nextTransform = world.GetComponent<TransformComponent>(nextTarget);
            transform.Position = lastHit.HitPoint;
            transform.Forward =
                (nextTransform.Position - lastHit.HitPoint).normalized;

            // 强制切换为直线轨迹
            trajectory.Type = TrajectoryType.Straight;
            trajectory.Origin = lastHit.HitPoint;
            trajectory.ElapsedTime = 0f;
            trajectory.PreviousPosition = lastHit.HitPoint;
        }

        /// <summary>
        /// 命中销毁前提交 OnExpire 效果。
        /// 必须在此处提交而非等 LifetimeSystem，
        /// 因为是碰撞触发的销毁而非时间过期。
        /// </summary>
        private void SubmitExpireAndDestroy(Entity bulletEntity,
            SourceInfoComponent sourceInfo, Vector3 hitPoint)
        {
            var payload = world.GetComponent<EffectPayloadComponent>(bulletEntity);
            if (payload != null)
            {
                var expireEffects = PayloadHelper.GetEffects(
                    payload, PayloadTrigger.OnExpire);
                if (expireEffects.Length > 0)
                {
                    var transform = world.GetComponent<TransformComponent>(
                        bulletEntity);
                    EffectSubmitHelper.Submit(effectBuffer,
                        sourceInfo.Source, bulletEntity, Entity.Null,
                        expireEffects, hitPoint,
                        transform != null ? transform.Forward : default);
                }
            }

            if (!world.HasComponent<DestroyTagComponent>(bulletEntity))
                world.AddComponent<DestroyTagComponent>(bulletEntity);
        }
    }
}