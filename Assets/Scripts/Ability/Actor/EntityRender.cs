using System;
using UnityEngine;

namespace Ability
{
    public class EntityRender : IEntity
    {
        public Entity LogicEntity;
        public GameObject gameObject;
        public void Bind(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
        public override void Init()
        {
            base.Init();
            LogicEntity = FightManager.LogicEntity.GetEntity(Uid);
        }
        public override void Destroy()
        {
            LogicEntity = null;
            base.Destroy();
        }
    }
}