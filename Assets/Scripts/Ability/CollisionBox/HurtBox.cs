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
        internal void OnHurt(ActorModel atkModel, Transform atkTrans, AbilityAttack atk)
        {
            // todo 状态统一管理
            if (atkModel.IsDead || atkModel.IsInvincible) return;

            var attackDir = (atkModel.transform.position - model.transform.position).normalized;
            float angle = Vector3.Angle(model.transform.forward, attackDir) * 2;
            if (angle <= model.GetCurAbilityAttack().BlockAngle)
            {
                //格挡成功
                foreach (var item in model.GetCurAbilityBehavior().BlockEvents)
                {
                    item.OnHurt(atkModel, model, atk, atkTrans);
                }
            }
            else
            {
                //受击成功
                foreach (var item in model.GetCurAbilityBehavior().HurtEvents)
                {
                    item.OnHurt(atkModel, model, atk, atkTrans);
                }
                ApplyHurtInfo(atkModel, atkTrans, atk);
            }
            model.DeathCheck();
        }

        private void ApplyHurtInfo(ActorModel atkModel, Transform atkTrans, AbilityAttack atk)
        {
            throw new NotImplementedException();
        }
    }
}
