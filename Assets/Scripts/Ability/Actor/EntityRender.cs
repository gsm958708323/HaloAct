using System;
using UnityEngine;

namespace Ability
{
    public class EntityRender : IEntity
    {
        public Entity LogicEntity;
        public GameObject gameObject;

        public void BindGo(GameObject gameObject)
        {
            this.gameObject = gameObject;
            var idCard = gameObject.AddComponent<IdentitCard>();
            idCard.Uid = Uid;
        }
        public override void Init()
        {
            base.Init();
            LogicEntity = FightManager.LogicEntity.GetEntity(Uid);
        }
        public override void Destroy()
        {
            LogicEntity = null;
            if (gameObject is not null)
            {
                UnityEngine.Object.Destroy(gameObject);
                gameObject = null;
            }
            base.Destroy();
        }
    }
}
