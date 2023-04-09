using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCombo : MonoBehaviour
{
    private List<AbilityTimeline> comboList;
    private int comboIndex = 0;
    AbilityTimer comboTimer;

    internal void Init(KeyCode key, List<AbilityTimeline> comboList)
    {
        this.comboList = comboList;
        comboTimer = new AbilityTimer();
        comboTimer.TimeOutCB = OnTimeOut;
        InputMgr.Instance.AddKeyDown(key, OnKeyDown);
    }

    private void Update()
    {
        comboTimer.OnUpdate(Time.deltaTime);
    }

    private void OnKeyDown(KeyCode obj)
    {
        //打断上一个
        if (comboTimer.IsActive())
        {
            var pre = comboList[comboIndex];
            pre.Cancel();
            comboIndex = Mathf.Clamp(++comboIndex, 0, comboList.Count - 1);
        }

        //播放现在的
        print($"当前连招：{comboIndex}");
        var cur = comboList[comboIndex];
        cur.Execute(()=>{
            comboTimer.Start(1);
        });
    }

    private void OnTimeOut()
    {
        comboIndex = 0;
        print($"连招重置{comboIndex}");
    }
}
