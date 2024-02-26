using System;
using System.Collections.Generic;
using Ability;
using Frame;
using UnityEngine;

public class ActorManager : IManager
{
    private SortedDictionary<int, ActorModel> actorDict;
    Queue<int> addQue;
    public Action<ActorModel> OnAddActorEvent;

    public override void Enter()
    {
        base.Enter();

        actorDict = new SortedDictionary<int, ActorModel>();
        addQue = new Queue<int>();
    }

    public override void Exit()
    {
        foreach (var item in actorDict)
        {
            item.Value.Exit();
        }
        actorDict.Clear();

        base.Exit();
    }

    public void AddActor(int id)
    {
        if (actorDict.ContainsKey(id))
        {
            Debugger.LogError($"actor id 重复", LogDomain.Actor);
            return;
        }

        addQue.Enqueue(id);
    }

    public void OnAddActor(int id)
    {
        var path = $"Actor/{id}";
        var actorData = Resources.Load<ActorData>(path);
        if (actorData is null)
        {
            Debugger.LogError($"actor配置不存在 {path}", LogDomain.Actor);
            return;
        }

        var actorGo = GameObject.Instantiate(actorData.Prefab);
        var actorModel = actorGo.AddComponent<ActorModel>();
        actorModel.Init();
        actorModel.Enter(actorData);
        actorDict.Add(id, actorModel);

        var actorRender = actorGo.AddComponent<ActorRender>();
        actorRender.Bind(id);

        OnAddActorEvent?.Invoke(actorModel);
    }

    public void RemoveActor(int id)
    {
        if (!actorDict.ContainsKey(id))
        {
            Debugger.LogError($"actor id 不存在", LogDomain.Actor);
            return;
        }

        var actorModel = actorDict[id];
        actorModel.Exit();

        actorDict.Remove(id);
    }

    public ActorModel GetActor(int id)
    {
        if (!actorDict.ContainsKey(id))
            return null;
        return actorDict[id];
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        // action在tick时可能会创建子弹，执行AddActor方法，这是不允许的
        foreach (var item in actorDict)
        {
            item.Value.Tick(deltaTime);
        }

        while (addQue.Count > 0)
        {
            var actorId = addQue.Dequeue();
            OnAddActor(actorId);
        }
    }
}