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
    public class Entity : ILogicT<object>
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
        Dictionary<Type, IComponent> compDic;

        public int Uid;
        public GameObject gameObject;

        Entity creater;
        /// <summary>
        /// 目标
        /// </summary>
        public Entity Target;

        public bool IsDead;
        public bool IsInvincible;

        public Transform transform
        {
            get
            {
                return gameObject.transform;
            }
        }

        public void Init()
        {

        }

        public T AddComp<T>() where T : IComponent, new()
        {
            var comp = new T();
            compList.Add(comp);
            compDic.Add(typeof(T), comp);

            comp.Init();
            comp.Enter(this);
            return comp;
        }

        public T GetComp<T>() where T : IComponent
        {
            var type = typeof(T);
            if (compDic.ContainsKey(type))
            {
                return compDic[type] as T;
            }
            return null;
        }

        public void Enter(object t) { }

        public void Enter(object data, GameObject actorGo)
        {
            compList = new();
            compDic = new();
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
            compDic = new();
        }

        internal void DeathCheck()
        {

        }
    }
}