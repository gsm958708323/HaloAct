using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class IEntity
    {
        List<IComponent> compList;
        Dictionary<Type, IComponent> compDic;

        public T AddComp<T>() where T : IComponent, new()
        {
            var comp = new T();
            compList.Add(comp);
            compDic.Add(typeof(T), comp);

            comp.Init();
            comp.Enter(this);
            return comp;
        }

        public void RemoveComp<T>() where T : IComponent
        {
            var type = typeof(T);
            compDic.TryGetValue(type, out var comp);
            if (comp == null)
                return;

            comp.Exit();
            compDic.Remove(type);
            compList.Remove(comp);
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

        public int Uid;

        public void Bind(int uid)
        {
            this.Uid = uid;
        }

        public virtual void Init()
        {
            compList = new();
            compDic = new();
        }

        public virtual void Destroy()
        {
            compList = null;
            compDic = null;
        }

        public virtual void Enter()
        {

        }

        public virtual void Tick(float deltaTime)
        {
            foreach (var item in compList)
            {
                item.Tick(deltaTime);
            }
        }

        public virtual void Exit()
        {
            foreach (var item in compList)
            {
                item.Exit();
            }
            compList = null;
            compDic = new();
        }
    }
}