using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class HurtBox : AbilityBox
    {
        public Vector3 HitPoint { get; internal set; }

        /// <summary>
        /// 被攻击，传入攻击者数据
        /// </summary>
        /// <param name="atk"></param>
        /// <param name="atkTrans"></param>
        /// <param name="atk"></param>
        internal void OnHurt(ActorModel atkModel, Transform atkTrans, AbilityBehaviorAttack atk)
        {
            // todo 状态统一管理
            if (atkModel.IsDead || atkModel.IsInvincible) return;
            var curBehavior = model.tree.GetCurAbilityBehavior();
            if (curBehavior == null) return;

            // 判断格挡成功：双方都是执行攻击行为，并且格挡角度符合条件
            var attackDir = (atkModel.transform.position - model.transform.position).normalized;
            float angle = Vector3.Angle(model.transform.forward, attackDir) * 2;
            if (angle <= curBehavior.BlockAngle && curBehavior is AbilityBehaviorAttack attackBehavior)
            {
                //格挡成功
                foreach (var item in attackBehavior.BlockEvents)
                {
                    item.OnHurt(atkModel, model, atk, atkTrans);
                }
            }
            else
            {
                //受到攻击
                ApplyHurtInfo(atkModel, atkTrans, atk);
            }
            model.DeathCheck();
        }

        private void ApplyHurtInfo(ActorModel atkModel, Transform atkTrans, AbilityBehaviorAttack attackBehavior)
        {
            AbilityNode node = model.tree.GetHurtBehavior(attackBehavior.CurAttack.AttackType);
            if (node == null) return;
            model.tree.StartBehavior(node);
        }
    }
}
