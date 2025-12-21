using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class HitBox : AbilityBox
    {
        private void OnTriggerEnter(Collider other)
        {
            if (model == null) return;

            var otherRender = other.GetComponent<ActorRender>();
            if (otherRender.uid == model.Uid) return; // 排除自己
            // if (model.Creater.Uid == otherRender.uid) return; // 排除创建者

            if (other.GetComponentInChildren<HurtBox>().gameObject.layer != LayerMask.NameToLayer("HurtBox"))
                return; // 只检测HurtBox

            var otherModel = otherRender.actorModel;
            if (otherModel is null)
                return;

            if (model.GetComp<ActorDataComp>().Data.ActorType == otherModel.GetComp<ActorDataComp>().Data.ActorType)
                return; // 排除同类

            OnHit(otherModel);
        }

        private void OnHit(ActorModel otherModel)
        {
            var otherHurtBox = otherModel.HurtBox;
            var behaviorComp = model.GetComp<ActorBehaviorComp>();
            if (behaviorComp is null)
                return;
            if (behaviorComp.GetCurAbilityBehavior() is not AbilityBehaviorAttack)
                return;

            AbilityBehaviorAttack attackBehavior = behaviorComp.GetCurAbilityBehavior() as AbilityBehaviorAttack;
            model.Target = otherModel;
            otherModel.Target = model;
            otherHurtBox.HitPoint = model.HitBox.transform.position;
            otherHurtBox.OnHurt(model, transform, attackBehavior);
        }
    }
}
