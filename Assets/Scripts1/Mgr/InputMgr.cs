using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InputMgr : MonoSingleton<InputMgr>
{
    Dictionary<KeyCode, List<Action<KeyCode>>> upDict = new Dictionary<KeyCode, List<Action<KeyCode>>>();

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (var item in upDict)
            {
                if (Input.GetKeyDown(item.Key))
                {
                    var cbList = upDict[item.Key];
                    foreach (var callback in cbList)
                    {
                        callback(item.Key);
                    }
                }
            }
        }
    }

    private void Test1(KeyCode obj)
    {
        print(obj.ToString());
    }

    public void AddKeyDown(KeyCode code, Action<KeyCode> callback)
    {
        if (!upDict.ContainsKey(code))
        {
            upDict[code] = new List<Action<KeyCode>>();
        }

        upDict[code].Add(callback);
    }
}
