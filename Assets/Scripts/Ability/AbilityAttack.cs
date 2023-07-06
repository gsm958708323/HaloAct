using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class FrameInfo
    {
        public int StartFrame = 1;
        public int EndFrame = 60;
    }

    public class HitBoxInfo
    {
        public Vector3 HitBoxPos;
        public Quaternion HitBoxRot;
        public Vector3 HitBoxScale;
    }

    public enum AttackType
    {
        Normal,
        KnockDown,
        HitFly,
        Stun,
    }

    /// <summary>
    /// 攻击配置
    /// </summary>
    public class AbilityAttack : ILogicT<ActorModel>
    {
        public AttackType AttackType;
        public FrameInfo FrameInfo;
        public HitBoxInfo HitBoxInfo;

        /// <summary>
        /// 格挡角度
        /// </summary>
        /// <value></value>
        public float BlockAngle { get; internal set; }

        public virtual void Init()
        {

        }

        public virtual void Enter(ActorModel model)
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void Tick(int frame)
        {
        }
    }
}

