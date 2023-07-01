﻿
using UnityEngine;
// public abstract class ILogic<A, B> : MonoBehaviour
// {
//     protected A ctrl;
//     protected B parent;

//     public virtual void OnStart() { }
//     public virtual void OnUpdate() { }
//     public virtual void OnExit() { }
//     public virtual void Bind(A ctrl, B parent)
//     {
//         this.ctrl = ctrl;
//         this.parent = parent;
//     }
// }

public interface ILogic
{
    void Init();
    void Enter();
    void Tick();
    void Exit();
}

public interface ILogicT<T>
{
    void Init(T t);
    void Enter(T t);
    void Tick(T t);
    void Exit(T t);
}