using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

namespace Ability
{
    public class AoeManager : IManager
    {
        readonly Collider[] overlapBuffer = new Collider[64];
        readonly HashSet<int> currentActorTargets = new();
        readonly HashSet<int> currentBulletTargets = new();
        readonly List<Entity> actorEntities = new();
        readonly List<Entity> bulletEntities = new();

        public override int Priority => -10;

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            var list = FightManager.LogicEntity.GetEntityLinkedList(EntityType.Aoe);
            if (list is null)
            {
                return;
            }

            var node = list.First;
            while (node != null)
            {
                var current = node;
                node = node.Next;

                var aoe = current.Value as Entity;
                if (aoe == null)
                {
                    continue;
                }

                var comp = aoe.GetComp<AoeDataComp>();
                if (comp == null || comp.Data == null)
                {
                    FightManager.LogicEntity.RemoveEntity(aoe.Uid);
                    continue;
                }

                comp.Move(deltaTime);
                ProcessTargets(aoe, comp, deltaTime);

                comp.TimeElapsed += deltaTime;
                if (comp.Duration > 0f && comp.TimeElapsed >= comp.Duration)
                {
                    RemoveAoe(aoe, comp);
                }
            }
        }

        void ProcessTargets(Entity aoe, AoeDataComp comp, float deltaTime)
        {
            currentActorTargets.Clear();
            currentBulletTargets.Clear();
            actorEntities.Clear();
            bulletEntities.Clear();

            var colliderCount = Physics.OverlapSphereNonAlloc(
                comp.Position,
                comp.Radius,
                overlapBuffer,
                ~0,
                QueryTriggerInteraction.Collide);
            for (int i = 0; i < colliderCount; i++)
            {
                if (!TryGetTarget(aoe, comp, overlapBuffer[i], out var target))
                {
                    continue;
                }

                switch (target.EntityType)
                {
                    case EntityType.Actor:
                        if (currentActorTargets.Add(target.Uid))
                        {
                            actorEntities.Add(target);
                        }
                        break;
                    case EntityType.Bullet:
                        if (currentBulletTargets.Add(target.Uid))
                        {
                            bulletEntities.Add(target);
                        }
                        break;
                }
            }

            NotifyEnters(comp, actorEntities, currentActorTargets, true);
            NotifyEnters(comp, bulletEntities, currentBulletTargets, false);

            var tickCount = comp.ConsumeTickCount(deltaTime);
            for (int i = 0; i < tickCount; i++)
            {
                NotifyTicks(comp, actorEntities);
                NotifyTicks(comp, bulletEntities);
            }

            NotifyLeaves(comp, currentActorTargets, true);
            NotifyLeaves(comp, currentBulletTargets, false);
            comp.SyncTargets(currentActorTargets, currentBulletTargets);
        }

        void NotifyEnters(AoeDataComp comp, List<Entity> targets, HashSet<int> currentTargets, bool isActor)
        {
            if (comp.Data.OnEnter == null)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var wasTracked = isActor ? comp.HasActorTarget(target.Uid) : comp.HasBulletTarget(target.Uid);
                if (!wasTracked && currentTargets.Contains(target.Uid))
                {
                    comp.Data.OnEnter.Execute(comp, target);
                }
            }
        }

        void NotifyTicks(AoeDataComp comp, List<Entity> targets)
        {
            if (comp.Data.OnTick == null)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                comp.Data.OnTick.Execute(comp, targets[i]);
            }
        }

        void NotifyLeaves(AoeDataComp comp, HashSet<int> currentTargets, bool isActor)
        {
            if (comp.Data.OnLeave == null)
            {
                return;
            }

            var trackedTargets = isActor ? comp.ActorTargetsInRange : comp.BulletTargetsInRange;
            foreach (var uid in trackedTargets)
            {
                if (currentTargets.Contains(uid))
                {
                    continue;
                }

                var target = FightManager.LogicEntity.GetEntity(uid);
                if (target != null)
                {
                    comp.Data.OnLeave.Execute(comp, target);
                }
            }
        }

        bool TryGetTarget(Entity aoe, AoeDataComp comp, Collider collider, out Entity target)
        {
            target = null;
            var idCard = collider.GetComponentInParent<IdentitCard>();
            if (idCard == null)
            {
                return false;
            }

            target = FightManager.LogicEntity.GetEntity(idCard.Uid);
            if (target == null || target.Uid == aoe.Uid || target.EntityType == EntityType.Aoe)
            {
                return false;
            }

            return CanAffectTarget(comp, target);
        }

        bool CanAffectTarget(AoeDataComp comp, Entity target)
        {
            switch (target.EntityType)
            {
                case EntityType.Actor:
                    if (!comp.Data.AffectActors)
                    {
                        return false;
                    }

                    var casterData = comp.Caster?.GetComp<PlayerDataComp>()?.Data;
                    var targetData = target.GetComp<PlayerDataComp>()?.Data;
                    if (targetData == null)
                    {
                        return false;
                    }

                    if (casterData == null)
                    {
                        return comp.Data.HitAlly || comp.Data.HitFoe;
                    }

                    var isAlly = casterData.ActorType == targetData.ActorType;
                    return isAlly ? comp.Data.HitAlly : comp.Data.HitFoe;
                case EntityType.Bullet:
                    return comp.Data.AffectBullets;
                default:
                    return false;
            }
        }

        void RemoveAoe(Entity aoe, AoeDataComp comp)
        {
            comp.NotifyLeaveTrackedTargets();
            comp.NotifyRemoved();
            FightManager.LogicEntity.RemoveEntity(aoe.Uid);
        }
    }
}
