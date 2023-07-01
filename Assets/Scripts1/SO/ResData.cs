using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ResData", menuName = "GameData/ResData", order = 0)]
public class ResData : ScriptableObject
{
    public List<ResInfo> AbilityPrefabList;
}

[System.Serializable]
public struct ResInfo
{
    public int Id;
    public GameObject Prefab;
}