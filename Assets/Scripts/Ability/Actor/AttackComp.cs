using System;
using UnityEngine;

namespace Ability
{
    public class AttackComp : IComponent
    {
        public Vector3 Hitpoint;
        Entity entity;
        internal void Enter(Entity entity, HitBoxInfo hitBoxInfo)
        {
            this.entity = entity;
            UnityGameAPI.InitHitBox(entity.Uid, hitBoxInfo, OnHit);
        }

        public override void Exit()
        {
            UnityGameAPI.RemoveHitBox(entity.Uid);
        }

        void OnHit(Entity other)
        {
            if (other.Uid == entity.Uid)
                return;// 排除自己

            if (entity.GetComp<PlayerDataComp>().Data.ActorType == other.GetComp<PlayerDataComp>().Data.ActorType)
                return; // 排除同类

            var behaviorComp = entity.GetComp<BehaviorComp>();
            if (behaviorComp is null)
                return;
            if (behaviorComp.GetCurAbilityBehavior() is not AbilityBehaviorAttack)
                return;

            AbilityBehaviorAttack attackBehavior = behaviorComp.GetCurAbilityBehavior() as AbilityBehaviorAttack;
            entity.Target = other;
            other.Target = entity;

            var otherComp = other.GetComp<AttackComp>();
            if (otherComp is null)
                return;
            otherComp.OnHurt(entity, attackBehavior);
        }

        void OnHurt(Entity atkEntity, AbilityBehaviorAttack atk)
        {
            // todo 状态统一管理
            var comp = entity.GetComp<BehaviorComp>();
            if (comp is null)
                return;

            var curBehavior = comp.GetCurAbilityBehavior();
            if (curBehavior == null) return;
            var atkTrans = atkEntity.GetComp<TransfromComp>();
            var curTrans = entity.GetComp<TransfromComp>();
            if (atkTrans == null || curTrans == null) return;

            // 判断格挡成功：双方都是执行攻击行为，并且格挡角度符合条件
            var attackDir = (atkTrans.Position - curTrans.Position).normalized;
            float angle = Vector3.Angle(curTrans.forward, attackDir) * 2;
            if (angle < curBehavior.BlockAngle && curBehavior is AbilityBehaviorAttack attackBehavior)
            {
                //格挡成功
                foreach (var item in attackBehavior.BlockEvents)
                {
                    item.OnHurt(atkEntity, entity, atk);
                }
            }
            else
            {
                ApplyHurtInfo(atkEntity, atk);
            }
            entity.DeathCheck();
        }

        private void ApplyHurtInfo(Entity atkEntity, AbilityBehaviorAttack atk)
        {
            var behaviorComp = entity.GetComp<BehaviorComp>();
            if (behaviorComp is null)
                return;

            AbilityNode node = behaviorComp.GetHurtBehavior(atk.CurAttack.AttackType);
            if (node == null) return;
            behaviorComp.StartBehavior(node);
        }
    }
}
