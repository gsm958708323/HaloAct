
using UnityEngine;
public abstract class ILogic : MonoBehaviour
{
    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
    public virtual void Bind<T>(T t) { }
}