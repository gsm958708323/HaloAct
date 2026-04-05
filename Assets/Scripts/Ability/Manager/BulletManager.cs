using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using Mathf = UnityEngine.Mathf;

namespace Ability
{
    public class BulletManager : IManager
    {
        static readonly IComparer<RaycastHit> HitDistanceComparer = Comparer<RaycastHit>.Create((left, right) =>
        {
            return left.distance.CompareTo(right.distance);
        });

        readonly RaycastHit[] hitBuffer = new RaycastHit[64];

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            var list = FightManager.LogicEntity.GetEntityLinkedList(EntityType.Bullet);
            if (list is null)
                return;

            var node = list.First;
            while (node != null)
            {
                var current = node;
                node = node.Next;

                var bullet = current.Value as Entity;
                if (bullet is null)
                {
                    continue;
                }

                var comp = bullet.GetComp<BulletDataComp>();
                if (comp is null)
                {
                    FightManager.LogicEntity.RemoveEntity(bullet.Uid);
                    continue;
                }

                if (comp.Data is null)
                {
                    RemoveBullet(bullet, comp, BulletRemoveReason.InvalidState);
                    continue;
                }

                if (comp.Hp <= 0)
                {
                    RemoveBullet(bullet, comp, BulletRemoveReason.HitLimitReached);
                    continue;
                }

                var remainingLifetime = comp.Duration - comp.TimeElapsed;
                if (comp.Duration <= 0f || remainingLifetime <= 0f)
                {
                    RemoveBullet(bullet, comp, BulletRemoveReason.LifetimeEnded);
                    continue;
                }

                var activeDeltaTime = Mathf.Min(deltaTime, remainingLifetime);
                var lifetimeEndsThisTick = remainingLifetime <= deltaTime;
                var moveDistance = comp.Speed * activeDeltaTime;
                if (moveDistance > 0f)
                {
                    if (TryProcessCollision(bullet, comp, moveDistance, activeDeltaTime))
                    {
                        continue;
                    }

                    comp.Position += comp.Direction * moveDistance;
                }

                comp.TimeElapsed += activeDeltaTime;
                if (lifetimeEndsThisTick && comp.TimeElapsed >= comp.Duration)
                {
                    RemoveBullet(bullet, comp, BulletRemoveReason.LifetimeEnded);
                }
            }
        }

        private bool TryProcessCollision(Entity bullet, BulletDataComp comp, float moveDistance, float activeDeltaTime)
        {
            if (moveDistance <= 0f || activeDeltaTime <= 0f)
            {
                return false;
            }

            var direction = comp.Direction;
            if (direction == Vector3.zero)
            {
                direction = Vector3.forward;
            }
            direction.Normalize();

            if (!TryGetCollisionWindow(comp, direction, moveDistance, activeDeltaTime, out var castOrigin, out var castDistance))
            {
                return false;
            }

            var hitCount = Physics.SphereCastNonAlloc(
                castOrigin,
                comp.Data.Radius,
                direction,
                hitBuffer,
                castDistance,
                ~0,
                QueryTriggerInteraction.Collide);
            if (hitCount <= 0)
            {
                return false;
            }

            System.Array.Sort(hitBuffer, 0, hitCount, HitDistanceComparer);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitBuffer[i];
                var collider = hit.collider;
                if (collider is null)
                {
                    continue;
                }

                if (TryGetTarget(collider, out var target))
                {
                    if (target.Uid == bullet.Uid)
                    {
                        continue;
                    }

                    if (!CanHitTarget(comp, target))
                    {
                        continue;
                    }

                    if (!comp.CanHitTarget(target.Uid, comp.Data.HitSameDelay))
                    {
                        continue;
                    }

                    comp.RecordTargetHit(target.Uid);
                    if (FightManager.Damage != null && comp.Caster != null)
                    {
                        FightManager.Damage.Enqueue(new DamageInfo
                        {
                            Attacker = comp.Caster,
                            Defender = target,
                            Source = comp,
                            Tags = DamageTag.Bullet,
                            TriggerHurtBehavior = false,
                        });
                    }
                    comp.Data.OnHit?.Execute(comp, target);
                    comp.Hp -= 1;
                    if (comp.Hp <= 0)
                    {
                        RemoveBullet(bullet, comp, BulletRemoveReason.HitLimitReached);
                        return true;
                    }

                    continue;
                }

                if (IsObstacleCollider(collider) && comp.Data.RemoveOnObstacle)
                {
                    RemoveBullet(bullet, comp, BulletRemoveReason.ObstacleHit);
                    return true;
                }
            }

            return false;
        }

        bool TryGetCollisionWindow(BulletDataComp comp, Vector3 direction, float moveDistance, float activeDeltaTime, out Vector3 castOrigin, out float castDistance)
        {
            castOrigin = comp.Position;
            castDistance = moveDistance;

            if (comp.CanHitAfterCreated <= 0f || comp.TimeElapsed >= comp.CanHitAfterCreated)
            {
                return true;
            }

            var timeUntilCanHit = comp.CanHitAfterCreated - comp.TimeElapsed;
            if (timeUntilCanHit >= activeDeltaTime || comp.Speed <= 0f)
            {
                return false;
            }

            var skipDistance = comp.Speed * timeUntilCanHit;
            castOrigin += direction * skipDistance;
            castDistance -= skipDistance;
            return castDistance > 0f;
        }

        private bool TryGetTarget(Collider collider, out Entity target)
        {
            target = null;
            var hurtBox = collider.GetComponentInParent<HurtBox>();
            if (hurtBox is null)
            {
                return false;
            }

            var idCard = collider.GetComponentInParent<IdentitCard>();
            if (idCard is null)
            {
                return false;
            }

            target = FightManager.LogicEntity.GetEntity(idCard.Uid);
            return target is not null;
        }

        private bool CanHitTarget(BulletDataComp comp, Entity target)
        {
            var casterData = comp.Caster?.GetComp<PlayerDataComp>()?.Data;
            var targetData = target.GetComp<PlayerDataComp>()?.Data;
            if (casterData is null || targetData is null)
            {
                return false;
            }

            var isAlly = casterData.ActorType == targetData.ActorType;
            if (isAlly)
            {
                return comp.Data.HitAlly;
            }

            return comp.Data.HitFoe;
        }

        private bool IsObstacleCollider(Collider collider)
        {
            if (collider.isTrigger)
            {
                return false;
            }

            if (collider.GetComponentInParent<IdentitCard>() is not null)
            {
                return false;
            }

            return true;
        }

        private void RemoveBullet(Entity bullet, BulletDataComp comp, BulletRemoveReason reason)
        {
            comp.RemoveReason = reason;
            comp.Data?.OnRemoved?.Execute(comp, null);
            FightManager.LogicEntity.RemoveEntity(bullet.Uid);
        }
    }
}
