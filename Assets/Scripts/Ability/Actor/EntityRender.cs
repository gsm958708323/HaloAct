using System;
using UnityEngine;

namespace Ability
{
    public class EntityRender : IRender
    {
        CharacterController controller;

        protected override void OnAwake()
        {
            base.OnAwake();
            controller = gameObject.GetComponent<CharacterController>();
        }

        protected override void OnUpdate(Entity actorModel)
        {
            var transComp = actorModel.GetComp<TransfromComp>();
            if(transComp is null)
            {
                return;
            }

            controller.transform.position = transComp.Position;
            controller.transform.rotation = transComp.Rotation;
        }
    }

}