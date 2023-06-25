using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResMgr : MonoSingleton<ResMgr>
{
    Dictionary<int, AbilityTimeline> abilityDict = new Dictionary<int, AbilityTimeline>();
    ResData resData;

    protected override void Awake()
    {
        base.Awake();
        resData = Resources.Load<ResData>("SO/ResData");
    }

    public AbilityTimeline GetAbility(int id)
    {
        if (abilityDict.ContainsKey(id))
        {
            return abilityDict[id];
        }

        foreach (var item in resData.AbilityPrefabList)
        {
            if (item.Id == id)
            {
                GameObject go = GameObject.Instantiate(item.Prefab);
                AbilityTimeline timeline = go.GetComponent<AbilityTimeline>();
                abilityDict.Add(id, timeline);
                return timeline;
            }
        }
        return null;
    }
}
