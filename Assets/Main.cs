using System;
using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

public class Main : MonoBehaviour
{
    public CameraMgr cameraMgr;
    const int PLAYER = 1001;
    const int MONSTER = 2001;

    // Start is called before the first frame update
    void Start()
    {
        var player = GameManager.ActorManager.AddActor(PLAYER);
        cameraMgr.BindInput(player.GameInput);
        GameManager.ActorManager.AddActor(MONSTER);
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
