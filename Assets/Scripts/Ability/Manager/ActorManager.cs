using System;
using System.Collections.Generic;
using Ability;
using Frame;
using UnityEngine;

public class ActorManager : IManager
{
    private LinkedList<ActorModel> actorList;
    private Dictionary<int, ActorModel> actorDict;

    public override void Enter()
    {
        base.Enter();

        actorList = new();
        actorDict = new();
    }

    public override void Exit()
    {
        foreach (var item in actorList)
        {
            item.Exit();
        }
        actorList.Clear();
        actorDict.Clear();

        base.Exit();
    }

    public ActorModel AddActor(int id)
    {
        if (actorDict.ContainsKey(id))
        {
            return actorDict[id];
        }

        var actorData = GameManager.Config.LoadActor(id);
        if (actorData is null)
        {
            return null;
        }

        var actorGo = GameObject.Instantiate(actorData.Prefab);
        var actorModel = actorGo.AddComponent<ActorModel>();
        actorModel.Init();
        actorModel.Enter(actorData);
        var actorRender = actorGo.AddComponent<ActorRender>();
        actorRender.Bind(id);

        actorDict.Add(id, actorModel);
        actorList.AddLast(new LinkedListNode<ActorModel>(actorModel));

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
        actorList.Remove(actorModel);
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

        // foreach (var actor in actorList)
        // {
        //     actor.Tick(deltaTime);
        // }

        var node = actorList.First;
        while(node != null)
        {
            node.Value.Tick(deltaTime);
            node = node.Next;
        }
    }
}