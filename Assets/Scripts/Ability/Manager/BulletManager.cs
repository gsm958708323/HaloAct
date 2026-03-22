using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

namespace Ability
{
    public class BulletManager : IManager
    {
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
                if (comp is null || comp.Data is null)
                {
                    FightManager.LogicEntity.RemoveEntity(bullet.Uid);
                    continue;
                }

                if (comp.Hp <= 0)
                {
                    RemoveBullet(bullet, comp);
                    continue;
                }

                comp.TimeElapsed += deltaTime;
                if (comp.Duration <= 0 || comp.TimeElapsed >= comp.Duration)
                {
                    RemoveBullet(bullet, comp);
                    continue;
                }

                var moveDistance = comp.Speed * deltaTime;
                if (moveDistance <= 0)
                {
                    continue;
                }

                if (TryProcessCollision(bullet, comp, moveDistance))
                {
                    continue;
                }

                comp.Position += comp.Direction * moveDistance;
            }
        }

        private bool TryProcessCollision(Entity bullet, BulletDataComp comp, float moveDistance)
        {
            var direction = comp.Direction;
            if (direction == Vector3.zero)
            {
                direction = Vector3.forward;
            }
            direction.Normalize();

            var hits = Physics.SphereCastAll(comp.Position, comp.Data.Radius, direction, moveDistance, ~0, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
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
                    comp.Data.OnHit?.Execute(comp, target);
                    comp.Hp -= 1;
                    if (comp.Hp <= 0)
                    {
                        RemoveBullet(bullet, comp);
                        return true;
                    }

                    continue;
                }

                if (IsObstacleCollider(collider) && comp.Data.RemoveOnObstacle)
                {
                    RemoveBullet(bullet, comp);
                    return true;
                }
            }

            return false;
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

        private void RemoveBullet(Entity bullet, BulletDataComp comp)
        {
            comp.Data?.OnRemoved?.Execute(comp, null);
            FightManager.LogicEntity.RemoveEntity(bullet.Uid);
        }
    }
}

