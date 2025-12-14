using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class IRender : MonoBehaviour
    {
        protected int actorId;
        protected ActorManager actorManager;
        private ActorModel actorModel;

        private void Awake()
        {
            actorManager = GameManager.Actor;
            OnAwake();
        }

        public void Bind(int entityId)
        {
            actorId = entityId;
        }

        // Update is called once per frame
        void Update()
        {
            var actor = actorManager.GetActor(actorId);
            if (actor == null)
            {
                // 之前有数据，现在没有数据，需要调用销毁逻辑
                if (actorModel != null)
                {
                    GameObject.Destroy(gameObject);
                }
            }
            else
            {
                OnUpdate(actor);
            }

            actorModel = actor;
        }

        protected virtual void OnUpdate(ActorModel model)
        {
        }

        protected virtual void OnAwake()
        {
        }
    }
}
