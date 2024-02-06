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
            if (actorModel.Velocity != Vector3.zero)
            {
                Debug.Log(actorModel.Velocity);
            }
            controller.Move(actorModel.Velocity * GameManager.Instance.FrameRate);
            controller.transform.rotation = actorModel.Rotation;
        }
    }

}