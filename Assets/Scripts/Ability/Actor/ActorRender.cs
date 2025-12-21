using System;
using UnityEngine;

namespace Ability
{
    public class ActorRender : IRender
    {
        CharacterController controller;

        protected override void OnAwake()
        {
            base.OnAwake();
            controller = gameObject.GetComponent<CharacterController>();
        }

        protected override void OnUpdate(ActorModel actorModel)
        {
            controller.transform.position = actorModel.Position;
            controller.transform.rotation = actorModel.Rotation;
        }
    }

}