using System;
using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

public class Main : MonoBehaviour
{
    public CameraMgr cameraMgr;

    // Start is called before the first frame update
    void Start()
    {
        // 下一帧才添加成功，
        GameManager.ActorManager.OnAddActorEvent += OnAddActorEvent;
        GameManager.ActorManager.AddActor(1001);
        GameManager.ActorManager.AddActor(2001);
    }

    private void OnAddActorEvent(int actorId)
    {
        if (actorId == 1001)
        {
            var actor = GameManager.ActorManager.GetActor(actorId);
            cameraMgr.BindInput(actor.GameInput);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    /*
        BulletManager
        BuffManager
        ActorManger （管理所有actorModel）
            HitBox
            HurtBox
            AbilityTree
        考虑位置如何赋值 （表现层绑定逻辑层数据）
            characterController 从actorModel中分离
            ActorRender
            ActorModel
        tick时间统一驱动
    */
}
