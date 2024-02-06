using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.ActorManager.AddActor(1001);
        GameManager.ActorManager.AddActor(2001);
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
