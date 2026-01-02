using System;
using System.Collections.Generic;
using Ability;
using HaloFrame;
using Unity.VisualScripting;
using UnityEngine;

public class EntityManager : IManager
{
    private LinkedList<Entity> actorList;
    private Dictionary<int, Entity> actorDict;
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

    Entity AddActor()
    {
        var id = idCreate.Get();
        var actor = new Entity() { Uid = id };
        actorList.AddLast(new LinkedListNode<Entity>(actor));
        actorDict.Add(id, actor);
        return actor;
    }

    public Entity CreateEntity(int cfgId)
    {
        var data = GameManager.Config.LoadActor(cfgId);
        if (data is null)
        {
            return null;
        }

        var actor = AddActor();
        var actorGo = GameObject.Instantiate(data.Prefab);
        actor.Init();
        actor.Enter(data, actorGo);

        var dataComp = actor.AddComp<PlayerDataComp>();
        dataComp.Data = data;
        actor.AddComp<TransfromComp>();
        actor.AddComp<BehaviorComp>();
        actor.AddComp<EffectComp>();

        var render = actorGo.AddComponent<EntityRender>();
        render.Bind(actor.Uid);
        var checker = render.AddComponent<GroundChecker>();
        checker.Init(actor);
        // gsm todo
        var hitBox = render.GetComponentInChildren<HitBox>();
        hitBox.Init();
        hitBox.Exit(); // 默认隐藏
        var hurtBox = render.GetComponentInChildren<HurtBox>();
        hurtBox.Init();
        hurtBox.Enter(actor);

        GameManager.Dispatcher.Notify(EventId.CreateEntity, actor);
        return actor;
    }


    public Entity CreateBullet(BulletLauncher launcher)
    {
        var id = launcher.BulletId;
        var data = GameManager.Config.LoadBullet(id);
        if (data is null)
        {
            return null;
        }

        var actorGo = GameObject.Instantiate(data.Prefab);
        var actor = AddActor();
        actor.Enter(data, actorGo);
        var dataComp = actor.AddComp<BulletDataComp>();
        dataComp.Data = data;

        var actorRender = actorGo.AddComponent<EntityRender>();
        actorRender.Bind(actor.Uid);

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

    public Entity GetEntity(int uid)
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