using System;
using System.Collections.Generic;
using Ability;
using Frame;
using UnityEngine;

public class ActorManager : IManager
{
    public SortedDictionary<int, ActorModel> actorDict;
    public override void Enter()
    {
        base.Enter();

        actorDict = new SortedDictionary<int, ActorModel>();
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

    public ActorModel AddActor(int id)
    {
        if (actorDict.ContainsKey(id))
        {
            Debugger.LogError($"actor id 重复", LogDomain.Actor);
            return actorDict[id];
        }

        var path = $"Actor/{id}";
        var actorData = Resources.Load<ActorData>(path);
        if (actorData is null)
        {
            Debugger.LogError($"actor配置不存在 {path}", LogDomain.Actor);
            return null;
        }

        var actorGo = GameObject.Instantiate(actorData.Prefab);
        actorGo.transform.position = actorData.BornPos;
        var actorModel = actorGo.AddComponent<ActorModel>();
        actorModel.Init();
        actorModel.Enter(actorData);
        actorDict.Add(id, actorModel);

        var actorRender = actorGo.AddComponent<ActorRender>();
        actorRender.Bind(id);
        return actorModel;
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

        foreach (var item in actorDict)
        {
            item.Value.Tick(deltaTime);
        }
    }
}