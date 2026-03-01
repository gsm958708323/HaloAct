using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

namespace Ability
{
    public class BulletManager : IManager
    {
        enum BulletRemoveReason
        {
            HitExhausted,
            Timeout,
            Obstacle,
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            var list = FightManager.LogicEntity.GetEntityLinkedList(EntityType.Bullet);
            if (list is null)
                return;

            var node = list.First;
            while (node != null)
            {
                var next = node.Next;

                var bullet = node.Value as Entity;
                if (bullet is null)
                {
                    node = next;
                    continue;
                }

                var comp = bullet.GetComp<BulletDataComp>();
                var data = comp?.Data;
                var trans = bullet.GetComp<SimpleTransformComp>();
                if (comp is null || data is null || trans is null)
                {
                    node = next;
                    continue;
                }

                if (comp.Hp <= 0)
                {
                    RemoveBullet(bullet, comp, BulletRemoveReason.HitExhausted);
                    node = next;
                    continue;
                }

                comp.TimeElapsed += deltaTime;

                if (comp.Duration > 0 && comp.TimeElapsed >= comp.Duration)
                {
                    RemoveBullet(bullet, comp, BulletRemoveReason.Timeout);
                    node = next;
                    continue;
                }

                if (TryMoveBullet(trans, comp, data, deltaTime))
                {
                    RemoveBullet(bullet, comp, BulletRemoveReason.Obstacle);
                    node = next;
                    continue;
                }

                node = next;
            }
        }

        public void OnHit(int bulletUid, Entity other)
        {
            if (other is null)
                return;

            var bullet = FightManager.LogicEntity.GetEntity(bulletUid);
            if (bullet is null)
                return;

            var comp = bullet.GetComp<BulletDataComp>();
            var data = comp?.Data;
            if (comp is null || data is null)
                return;

            if (comp.Hp <= 0)
                return;

            if (other.Uid == bulletUid)
                return;

            if (comp.Caster is not null && other.Uid == comp.Caster.Uid)
                return;

            if (!CanHit(comp, other))
                return;

            if (data.HitSameDelay > 0)
            {
                if (comp.LastHitTimeByTarget != null && comp.LastHitTimeByTarget.TryGetValue(other.Uid, out var lastTime))
                {
                    if (comp.TimeElapsed - lastTime < data.HitSameDelay)
                        return;
                }

                comp.LastHitTimeByTarget ??= new Dictionary<int, float>();
                comp.LastHitTimeByTarget[other.Uid] = comp.TimeElapsed;
            }

            ApplyHit(comp, other);
            data.OnHit?.Execute();

            comp.Hp -= 1;
            if (comp.Hp <= 0)
            {
                RemoveBullet(bullet, comp, BulletRemoveReason.HitExhausted);
            }
        }

        static bool CanHit(BulletDataComp bulletComp, Entity other)
        {
            if (other.IsDead || other.IsInvincible)
                return false;

            // Only hit actor entities.
            var otherData = other.GetComp<PlayerDataComp>()?.Data;
            if (otherData is null)
                return false;

            var caster = bulletComp.Caster;
            var casterData = caster?.GetComp<PlayerDataComp>()?.Data;
            if (casterData is null)
            {
                // No caster info: default to allow.
                return true;
            }

            bool isAlly = casterData.ActorType == otherData.ActorType;
            return isAlly ? bulletComp.Data.HitAlly : bulletComp.Data.HitFoe;
        }

        static void ApplyHit(BulletDataComp bulletComp, Entity other)
        {
            if (bulletComp.Caster is not null)
            {
                bulletComp.Caster.Target = other;
                other.Target = bulletComp.Caster;
            }

            var behaviorComp = other.GetComp<BehaviorComp>();
            if (behaviorComp is null)
                return;

            var node = behaviorComp.GetHurtBehavior(bulletComp.Data.AttackType);
            if (node is null)
                return;

            behaviorComp.StartBehavior(node);
        }

        static bool TryMoveBullet(SimpleTransformComp trans, BulletDataComp comp, BulletData data, float deltaTime)
        {
            var forward = trans.forward;
            var step = comp.Speed * deltaTime;
            if (step <= 0)
                return false;

            var prevPos = trans.Position;
            var nextPos = prevPos + forward * step;

            if (data.RemoveOnObstacle && data.Radius > 0)
            {
                int groundMask = LayerMask.GetMask("Ground");
                if (groundMask != 0)
                {
                    var dir = nextPos - prevPos;
                    var dist = dir.magnitude;
                    if (dist > 0)
                    {
                        dir /= dist;
                        if (Physics.SphereCast(prevPos, data.Radius, dir, out _, dist, groundMask, QueryTriggerInteraction.Ignore))
                        {
                            return true;
                        }
                    }
                }
            }

            trans.Position = nextPos;
            return false;
        }

        static void RemoveBullet(Entity bullet, BulletDataComp comp, BulletRemoveReason reason)
        {
            if (bullet is null || comp is null)
                return;

            var data = comp.Data;
            UnityGameAPI.RemoveHitBox(bullet.Uid);
            data?.OnRemoved?.Execute();

            FightManager.RenderEntity.RemoveEntity(bullet.Uid);
            FightManager.LogicEntity.RemoveEntity(bullet.Uid);
        }
    }
}

