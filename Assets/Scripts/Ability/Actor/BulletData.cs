using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/BuffData")] 
public class BulletData : SerializedScriptableObject
{
    public int Id;
    public GameObject Prefab;
    public float Radius;
    /// <summary>
    /// 子弹可以碰撞的次数，每次碰到目标-1
    /// </summary>
    public int HitTimes;
    /// <summary>
    /// 碰撞到同一个目标的延迟
    /// </summary>
    public float HitSameDelay;
}