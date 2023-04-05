using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "GameData/AbilityData", order = 0)]
public class AbilityData : ScriptableObject
{
    public string Name;
    public KeyCode BindingKey;
    public int AbilityResId;
}