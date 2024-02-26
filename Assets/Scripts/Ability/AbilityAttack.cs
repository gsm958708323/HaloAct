using System;
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
        public Vector3 HitBoxScale = Vector3.one;
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
    ///                 -> HitBox
    /// AbilityAttack ->
    ///                 -> HurtBox
    /// </summary>
    public class AbilityAttack : ILogicT<ActorModel>
    {
        public AttackType AttackType;
        public FrameInfo FrameInfo = new FrameInfo();
        public HitBoxInfo HitBoxInfo = new HitBoxInfo();

        HitBox hitBox;
        bool isEnter;

        public virtual void Init()
        {

        }

        public virtual void Enter(ActorModel model)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.AbilityAttack);
            isEnter = true;
            this.hitBox = model.HitBox;
            hitBox.Enter(model, HitBoxInfo.HitBoxPos, HitBoxInfo.HitBoxScale, HitBoxInfo.HitBoxRot);
        }

        public virtual void Exit()
        {
            Debugger.Log($"Exit {GetType()}", LogDomain.AbilityAttack);
            hitBox.Exit();
            hitBox = null;
            isEnter = false;
        }

        public virtual void Tick(float deltaTime)
        {
            hitBox.Tick(deltaTime);
        }

        internal bool IsEnter()
        {
            return isEnter;
        }
    }
}

