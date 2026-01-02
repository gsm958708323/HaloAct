using System;
using UnityEngine;

namespace Ability
{
    public class UnityGameAPI
    {
        public static void SetBox()
        {

        }

        public static bool CheckGround()
        {
            return false;
        }

        public static GameObject GetGameObjectByUid(int uid)
        {
            var entity = GameManager.LogicEntity.GetEntity(uid);
            if (entity is null)
                return null;
            return entity.gameObject;
        }
    }
}
