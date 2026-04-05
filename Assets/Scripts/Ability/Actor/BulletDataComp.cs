using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class BulletDataComp : ComponentLogic
    {
        public BulletData Data;
        public Entity Caster;
        public Vector3 Position;
        public Vector3 Direction;
        public float Speed;
        /// <summary>
        /// 子弹生存时间
        /// </summary>
        public float Duration;
        /// <summary>
        /// 子弹存活时间
        /// </summary>
        public float TimeElapsed;
        public float CanHitAfterCreated;
        public BulletRemoveReason RemoveReason { get; internal set; }

        public int Hp { get; internal set; }

        Dictionary<int, float> targetHitRecord;

        public override void Init()
        {
            targetHitRecord = new Dictionary<int, float>();
            base.Init();
        }

        public override void Destroy()
        {
            targetHitRecord = null;
            base.Destroy();
        }

        public bool CanHitTarget(int targetUid, float hitSameDelay)
        {
            if (hitSameDelay <= 0)
            {
                return true;
            }

            if (!targetHitRecord.TryGetValue(targetUid, out var lastHitTime))
            {
                return true;
            }

            return TimeElapsed - lastHitTime >= hitSameDelay;
        }

        public void RecordTargetHit(int targetUid)
        {
            targetHitRecord[targetUid] = TimeElapsed;
        }

        public bool IsInHitWindow()
        {
            return TimeElapsed >= CanHitAfterCreated;
        }
    }

    public struct BulletLauncher
    {
        public int BulletId;
        public BulletData Data;
        public Entity Caster;
        public Vector3 Position;
        public Vector3 Direction;
        public float? SpeedOverride;
        public float? DurationOverride;
        public float? CanHitAfterCreatedOverride;
    }
}
