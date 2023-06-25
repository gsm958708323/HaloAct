
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
    void OnInit();
    void OnEnter();
    void OnTick();
    void OnExit();
}

public interface ILogicT<T>
{
    void OnInit(T t);
    void OnEnter(T t);
    void OnTick(T t);
    void OnExit(T t);
}