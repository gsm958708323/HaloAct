using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class IRender : MonoBehaviour
    {
        public int uid;
        public Entity actorModel;

        private void Awake()
        {
            OnAwake();
        }

        public void Bind(int entityId)
        {
            uid = entityId;
        }

        // Update is called once per frame
        void Update()
        {
            var actor = GameManager.Actor.GetActor(uid);
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

        protected virtual void OnUpdate(Entity model)
        {
        }

        protected virtual void OnAwake()
        {
        }
    }
}
