using System;
using UnityEngine;

namespace Ability
{
    public class UnityGameAPI
    {
        public static bool CheckGround(int uid)
        {
            var go = GetGameObjectByUid(uid);
            if (go is null)
                return false;
            var comp = go.GetComponent<GroundChecker>();
            if (comp == null)
            {
                comp = go.AddComponent<GroundChecker>();
                comp.Init(uid);
            }
            return comp.CheckGround();
        }

        public static GameObject GetGameObjectByUid(int uid)
        {
            var entity = FightManager.RenderEntity.GetEntity(uid);
            if (entity is null)
                return null;
            return entity.gameObject;
        }

        internal static void InitHitBox(int uid, HitBoxInfo hitBoxInfo, Action<Entity> onHit)
        {
            var go = GetGameObjectByUid(uid);
            if (go == null)
                return;
            var hitBox = go.GetComponentInChildren<HitBox>();

            hitBox.AddHitCB(hitBoxInfo, onHit);
        }

        public static void RemoveHitBox(int uid)
        {
            var go = GetGameObjectByUid(uid);
            if (go == null)
                return;
            var hitBox = go.GetComponentInChildren<HitBox>();
            hitBox.RemoveHitCB();
        }
    }
}
