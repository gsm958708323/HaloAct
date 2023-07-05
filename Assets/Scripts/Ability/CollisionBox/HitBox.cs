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

            var otherModel = other.GetComponentInParent<ActorModel>();
            if (otherModel == model) return; // 排除自己

            if (other.gameObject.layer != LayerMask.NameToLayer("HurtBox"))
                return; // 只检测HurtBox

            if (model.ActorType == otherModel.ActorType)
                return; // 排除同类

            OnHit(otherModel);
        }

        private void OnHit(ActorModel otherModel)
        {
            var otherHurtBox = otherModel.HurtBox;
            AbilityAttack atk = model.GetCurAbilityAttack();
            if (atk == null) return;
            model.Target = otherModel;
            otherHurtBox.HitPoint = model.HitBox.transform.position;
            otherHurtBox.OnHurt(model, transform, atk);
        }
    }
}
