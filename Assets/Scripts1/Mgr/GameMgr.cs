using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    void Awake()
    {
        gameObject.AddComponent<ResMgr>();
        gameObject.AddComponent<InputMgr>();
    }

    void Start()
    {
        var worldData = Resources.Load<WorldData>("SO/worldData");
        GameObject hero = CreatePlayer(worldData.Hero);
        var camera = Camera.main.GetComponent<ThirdPersonCamera>();
        camera.target = hero.transform;
        CreatePlayer(worldData.Entity);
    }

    GameObject CreatePlayer(PlayerInfo info)
    {
        GameObject player = GameObject.Instantiate(info.Prefab);
        player.transform.position = info.BornPoint;
        player.AddComponent<PlayerController2>().Init(info);
        return player;
    }
}
