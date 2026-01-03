using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class HitBox : AbilityBox
    {
        Action<Entity> hitCB;
        public void AddHitCB(HitBoxInfo hitBoxInfo, Action<Entity> cb)
        {
            // 触发器一直显示OnTriggerEnter只会触发一次，这里保证每次都会触发
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            transform.localPosition = hitBoxInfo.HitBoxPos;
            transform.localScale = hitBoxInfo.HitBoxScale;
            transform.localRotation = hitBoxInfo.HitBoxRot;

            hitCB = cb;
        }

        public void RemoveHitCB()
        {
            hitCB = null;
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (model == null) return;

            var otherRender = other.GetComponent<EntityRender>();

            if (other.GetComponentInChildren<HurtBox>().gameObject.layer != LayerMask.NameToLayer("HurtBox"))
                return; // 只检测HurtBox

            var otherModel = otherRender.LogicEntity;
            if (otherModel is null)
                return;
            hitCB?.Invoke(otherModel);
        }
    }
}
