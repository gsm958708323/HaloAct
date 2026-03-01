using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class BulletDataComp : ComponentLogic
    {
        public BulletData Data;
        public Entity Caster;
        public float FirDegree;
        public float Speed;
        /// <summary>
        /// 子弹生存时间
        /// </summary>
        public float Duration;
        /// <summary>
        /// 子弹存活时间
        /// </summary>
        public float TimeElapsed;

        public int Hp;

        // Key: target uid; Value: last hit time (in this bullet's TimeElapsed space)
        public Dictionary<int, float> LastHitTimeByTarget;

        public override void Init()
        {
            base.Init();
            LastHitTimeByTarget = new Dictionary<int, float>();
        }
    }

    public struct BulletLauncher
    {
        public int BulletId;

        public Entity Caster;
        public float FireDegree;

        public Vector3 Position;
        public Quaternion Rotation;
    }
}

