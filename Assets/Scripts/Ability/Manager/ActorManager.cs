using System;
using System.Collections.Generic;
using Ability;
using Frame;
using Unity.VisualScripting;
using UnityEngine;

public class ActorManager : IManager
{
    private LinkedList<ActorModel> actorList;
    private Dictionary<int, ActorModel> actorDict;
    private IdCreate idCreate = new();

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

    public ActorModel CreatePlayer(int cfgId)
    {
        var data = GameManager.Config.LoadActor(cfgId);
        if (data is null)
        {
            return null;
        }

        var uid = idCreate.Get();
        var actorGo = GameObject.Instantiate(data.Prefab);
        var actor = new ActorModel();
        actor.Init();
        actor.Enter(data, uid, actorGo);

        actor.AddComp<TransfromComp>();
        actor.AddComp<ActorBehaviorComp>();
        actor.AddComp<ActorBuffComp>();

        var render = actorGo.AddComponent<ActorRender>();
        render.Bind(uid);
        render.AddComponent<PlayerGameInput>();
        var checker = render.AddComponent<GroundChecker>();
        checker.Init(actor);
        var hitBox = render.GetComponentInChildren<HitBox>();
        hitBox.Init();
        hitBox.Exit(); // 默认隐藏
        var hurtBox = render.GetComponentInChildren<HurtBox>();
        hurtBox.Init();
        hurtBox.Enter(actor);

        actorDict.Add(uid, actor);
        actorList.AddLast(new LinkedListNode<ActorModel>(actor));
        return actor;
    }

    public void RemoveActor(int uid)
    {
        if (!actorDict.ContainsKey(uid))
        {
            Debugger.LogError($"actor id 不存在", LogDomain.Actor);
            return;
        }

        var actorModel = actorDict[uid];
        actorModel.Exit();
        actorDict.Remove(uid);
        actorList.Remove(actorModel);
    }

    public ActorModel GetActor(int uid)
    {
        if (!actorDict.ContainsKey(uid))
            return null;
        return actorDict[uid];
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        var node = actorList.First;
        while (node != null)
        {
            node.Value.Tick(deltaTime);
            node = node.Next;
        }
    }
}