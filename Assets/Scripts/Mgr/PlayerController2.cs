using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerController2 : MonoBehaviour
{
    public void Init(PlayerInfo info)
    {
        var timeline = gameObject.AddComponent<PlayerAbilityController>();
        timeline.Init(info.AbilityList);

        if (info.PlayerType == PlayerType.Hero)
        {
            gameObject.AddComponent<PlayerMoveController>();
        }
    }
}
