using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class AoeDataComp : ComponentLogic
    {
        public AoeData Data;
        public Entity Caster;
        public Vector3 Position;
        public Vector3 Velocity;
        public float Radius;
        public float Duration;
        public float TimeElapsed;
        public float TickInterval;

        readonly HashSet<int> actorTargetsInRange = new();
        readonly HashSet<int> bulletTargetsInRange = new();
        float tickElapsed;
        bool removedNotified;

        public IReadOnlyCollection<int> ActorTargetsInRange => actorTargetsInRange;
        public IReadOnlyCollection<int> BulletTargetsInRange => bulletTargetsInRange;

        public void Move(float deltaTime)
        {
            Position += Velocity * deltaTime;
        }

        public int ConsumeTickCount(float deltaTime)
        {
            if (TickInterval <= 0f)
            {
                return 1;
            }

            tickElapsed += deltaTime;
            var tickCount = 0;
            while (tickElapsed >= TickInterval)
            {
                tickElapsed -= TickInterval;
                tickCount += 1;
            }

            return tickCount;
        }

        public bool HasActorTarget(int uid)
        {
            return actorTargetsInRange.Contains(uid);
        }

        public bool HasBulletTarget(int uid)
        {
            return bulletTargetsInRange.Contains(uid);
        }

        public void SyncTargets(HashSet<int> actorTargets, HashSet<int> bulletTargets)
        {
            actorTargetsInRange.Clear();
            foreach (var uid in actorTargets)
            {
                actorTargetsInRange.Add(uid);
            }

            bulletTargetsInRange.Clear();
            foreach (var uid in bulletTargets)
            {
                bulletTargetsInRange.Add(uid);
            }
        }

        public void NotifyLeaveTrackedTargets()
        {
            NotifyLeaveSet(actorTargetsInRange);
            NotifyLeaveSet(bulletTargetsInRange);
            actorTargetsInRange.Clear();
            bulletTargetsInRange.Clear();
        }

        public void NotifyRemoved()
        {
            if (removedNotified)
            {
                return;
            }

            removedNotified = true;
            Data?.OnRemoved?.Execute(this, null);
        }

        public override void Exit()
        {
            NotifyLeaveTrackedTargets();
            NotifyRemoved();
            base.Exit();
        }

        void NotifyLeaveSet(HashSet<int> targets)
        {
            if (Data?.OnLeave == null)
            {
                return;
            }

            foreach (var uid in targets)
            {
                var target = FightManager.LogicEntity?.GetEntity(uid);
                if (target != null)
                {
                    Data.OnLeave.Execute(this, target);
                }
            }
        }
    }

    public struct AoeLauncher
    {
        public int AoeId;
        public AoeData Data;
        public Entity Caster;
        public Vector3 Position;
        public Vector3 Velocity;
    }
}
