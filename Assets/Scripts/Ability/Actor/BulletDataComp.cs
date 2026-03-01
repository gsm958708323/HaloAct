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

        public int Hp { get; internal set; }
    }

    public struct BulletLauncher
    {
        public int BulletId;
        public string Prefab;
    }
}

