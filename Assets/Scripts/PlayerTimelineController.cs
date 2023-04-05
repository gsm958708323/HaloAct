using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTimelineController : MonoBehaviour
{
    public void Init(List<AbilityData> abilityList)
    {
        foreach (var item in abilityList)
        {
            AbilityTimeline timeline = ResMgr.Instance.GetAbility(item.AbilityResId);
            timeline.Init(item, this);
        }
    }
}
