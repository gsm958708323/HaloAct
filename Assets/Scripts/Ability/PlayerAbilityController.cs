using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{
    Dictionary<KeyCode, List<AbilityTimeline>> comboDict = new Dictionary<KeyCode, List<AbilityTimeline>>();

    public void Init(List<AbilityData> abilityList)
    {
        foreach (var data in abilityList)
        {
            AbilityTimeline timeline = ResMgr.Instance.GetAbility(data.AbilityResId);
            timeline.Init(data, this);

            //绑定技能按键
            if (data.BindingKey != KeyCode.None)
            {
                if (!comboDict.ContainsKey(data.BindingKey))
                {
                    comboDict.Add(data.BindingKey, new List<AbilityTimeline>());
                }
                comboDict[data.BindingKey].Add(timeline);
            }
        }

        //连招列表，创建连招管理
        foreach (var item in comboDict)
        {
            if (item.Value.Count > 1)
            {
                var combo = gameObject.AddComponent<AbilityCombo>();
                combo.Init(item.Key, item.Value);
            }
            else
            {
                var timeline = item.Value[1];
                InputMgr.Instance.AddKeyDown(item.Key, (keyCode) =>
                {
                    timeline.Execute();
                });
            }
        }
    }
}