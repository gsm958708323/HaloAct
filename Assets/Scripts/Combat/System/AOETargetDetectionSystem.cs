using System;

namespace Combat
{
    public class AOETargetDetectionSystem : ISystem
    {
        private readonly EffectRequestBuffer effectBuffer;

        public AOETargetDetectionSystem(EffectRequestBuffer effectBuffer)
        {
            this.effectBuffer = effectBuffer;
        }

        public override void Tick(float delteTime)
        {
            var aoes = world.Query<TickReadyTagComponent, ColliderComponent,
                        TargetMemoryComponent>();
            foreach (var aoeEntity in aoes)
            {
                var collider = world.GetComponent<ColliderComponent>(aoeEntity);
                var memory = world.GetComponent<TargetMemoryComponent>(aoeEntity);
                var sourceInfo = world.GetComponent<SourceInfoComponent>(aoeEntity);
                var payload = world.GetComponent<EffectPayloadComponent>(aoeEntity);
                var aoeTransform = world.GetComponent<TransformComponent>(aoeEntity);

                if (sourceInfo == null || payload == null) continue;

                if (aoeTransform != null)
                    collider.Data.Center = aoeTransform.Position;

                // --- 检测当前区域内的目标 ---
                DetectTargets(aoeEntity, collider, aoeTransform, sourceInfo, memory);

                // --- Enter ---
                var enterEffects = PayloadHelper.GetEffects(payload, PayloadTrigger.OnEnter);
                foreach (var target in memory.Current)
                {
                    if (!memory.Previous.Contains(target))
                    {
                        EffectSubmitHelper.Submit(effectBuffer, sourceInfo.Source, aoeEntity, target, enterEffects);
                    }
                }

                // --- OnTick（所有当前目标）---
                var tickEffects = PayloadHelper.GetEffects(
                    payload, PayloadTrigger.OnTick);
                foreach (var target in memory.Current)
                {
                    EffectSubmitHelper.Submit(effectBuffer,
                        sourceInfo.Source, aoeEntity, target,
                        tickEffects);
                }

                // --- Exit ---
                var exitEffects = PayloadHelper.GetEffects(
                    payload, PayloadTrigger.OnExit);
                foreach (var target in memory.Previous)
                {
                    if (!memory.Current.Contains(target))
                    {
                        if (world.IsAlive(target))
                        {
                            EffectSubmitHelper.Submit(effectBuffer,
                                sourceInfo.Source, aoeEntity, target,
                                exitEffects);
                        }
                    }
                }

                // --- 交换 ---
                memory.SwapAndClear();
            }
        }

        private void DetectTargets(
            Entity aoeEntity,
            ColliderComponent aoeCollider,
            TransformComponent aoeTransform,
            SourceInfoComponent sourceInfo,
            TargetMemoryComponent memory)
        {
            // 遍历所有有碰撞体的实体
            var candidates = world.Query
            <ColliderComponent, TransformComponent, FactionComponent>();
            foreach (var candidate in candidates)
            {
                if (candidate == aoeEntity) continue;
                // 阵营过滤：AOE 只命中敌对
                if (!FactionHelper.IsHostile(world, sourceInfo.Source, candidate))
                    continue;

                var targetCollider = world.GetComponent<ColliderComponent>(candidate);
                var targetTransform = world.GetComponent<TransformComponent>(candidate);

                targetCollider.Data.Center = targetTransform.Position;

                // 重叠检测
                if (!CollisionMath.Overlap(aoeCollider.Data, targetCollider.Data))
                    continue;

                // 扇形检测
                if (aoeCollider.IsSectorBounding && aoeTransform != null)
                {
                    if (!CollisionMath.IsInSector(
                        aoeTransform.Position, aoeTransform.Forward, targetTransform.Position, aoeCollider.SectorHalfAngle
                    )) ;
                    continue;
                }

                memory.Current.Add(candidate);
            }
        }
    }
}