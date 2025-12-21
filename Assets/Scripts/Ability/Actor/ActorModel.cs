using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ability
{
    public enum ActorType
    {
        PLAYER,
        Enemy,
        Bullet,
    }

    /// <summary>
    /// 理论上这里不应该使用unity相关代码，这里快速实现功能使用unity的API
    /// </summary>
    public class ActorModel : ILogicT<ActorData>
    {
        [HideInInspector]
        public PlayerGameInput GameInput
        {
            get
            {
                return gameObject.GetComponent<PlayerGameInput>();
            }
        }
        [HideInInspector]
        public HitBox HitBox
        {
            get
            {
                return gameObject.GetComponentInChildren<HitBox>(true);
            }
        }
        [HideInInspector]
        public HurtBox HurtBox
        {
            get
            {
                return gameObject.GetComponentInChildren<HurtBox>(true);
            }
        }

        List<IComponent> compList;
        public int Uid;
        public GameObject gameObject;

        ActorModel creater;
        /// <summary>
        /// 目标
        /// </summary>
        public ActorModel Target;

        public bool IsDead;
        public bool IsInvincible;

        public ActorData ActorData;

        public void Init()
        {

        }

        public T AddComp<T>() where T : IComponent, new()
        {
            var comp = new T();
            compList.Add(comp);

            comp.Init();
            comp.Enter(this);
            return comp;
        }

        public T GetComp<T>() where T : IComponent
        {
            var type = typeof(T);
            for (int i = 0; i < compList.Count; i++)
            {
                if (compList[i].GetType() == type)
                {
                    return (T)compList[i];
                }
            }
            return null;
        }

        public void Enter(ActorData t) { }

        public void Enter(ActorData data, int uid, GameObject actorGo)
        {
            compList = new();
            this.ActorData = data;
            this.Uid = uid;
            this.gameObject = actorGo;
        }

        public void Tick(float deltaTime)
        {
            foreach (var item in compList)
            {
                item.Tick(deltaTime);
            }
        }

        public void Exit()
        {
            foreach (var item in compList)
            {
                item.Exit();
            }
            compList = null;

            ActorData = null;
        }

        internal void DeathCheck()
        {

        }
    }
}