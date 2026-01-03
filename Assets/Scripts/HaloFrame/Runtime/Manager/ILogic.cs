

public interface ILogic
{
    void Init();
    void Enter();
    void Tick(float deltaTime);
    void Exit();
    void Destroy();
}

public interface ILogicT<T>
{
    void Init();
    void Enter(T t);
    void Tick(float deltaTime);
    void Exit();
}