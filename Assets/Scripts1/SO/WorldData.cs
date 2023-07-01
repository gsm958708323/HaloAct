using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldData", menuName = "GameData/WorldData", order = 0)]
public class WorldData : ScriptableObject
{
    public PlayerInfo Hero;
    public PlayerInfo Entity;
}

[System.Serializable]
public struct PlayerInfo
{
    public PlayerType PlayerType;
    public List<AbilityData> AbilityList;
    public Vector3 BornPoint;
    public GameObject Prefab;
}

public enum PlayerType
{
    Hero,
    Enemy,
}