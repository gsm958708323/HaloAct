using System;
using UnityEngine;

namespace Ability
{
    public class ActorRender : IRender
    {
        CharacterController controller;
        GroundChecker groundChecker;

        protected override void OnAwake()
        {
            base.OnAwake();
            controller = gameObject.GetComponent<CharacterController>();
        }

        protected override void OnUpdate(ActorModel actorModel)
        {
            // controller.transform.position = actorModel.Position;
            // controller.Move(actorModel.Velocity * GameManager.Instance.FrameRate);
            // controller.transform.rotation = actorModel.Rotation;

            // if (groundChecker == null)
            // {
            //     groundChecker = gameObject.AddComponent<GroundChecker>();
            //     groundChecker.Init(actorModel.ActorData.CheckerData);
            // }
            // actorModel.IsGround = groundChecker.CheckGround();
        }
    }

}