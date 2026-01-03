using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    [Serializable]
    public class FrameInfo
    {
        public int StartFrame = 1;
        public int EndFrame = 60;
    }

    [Serializable]
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
    public class AbilityAttack : ILogicT<Entity>
    {
        public AttackType AttackType;
        public FrameInfo FrameInfo;
        public HitBoxInfo HitBoxInfo;
        bool isEnter;
        Entity entity;

        public virtual void Init()
        {

        }

        public virtual void Enter(Entity entity)
        {
            Debugger.Log($"Enter {GetType()}", LogDomain.AbilityAttack);
            isEnter = true;

            this.entity = entity;
            var comp = entity.AddComp<AttackComp>();
            comp.Enter(entity, HitBoxInfo);
        }

        public virtual void Exit()
        {
            Debugger.Log($"Exit {GetType()}", LogDomain.AbilityAttack);
            entity.RemoveComp<AttackComp>();
            isEnter = false;
        }

        internal bool IsEnter()
        {
            return isEnter;
        }

        public void Tick(float deltaTime)
        {
            
        }
    }
}

