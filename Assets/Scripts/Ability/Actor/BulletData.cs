using System.Collections;
using System.Collections.Generic;
using Ability;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/BuffData")]
public class BulletData : SerializedScriptableObject
{
    [MinValue(1)]
    public int Id;
    public GameObject Prefab;
    [MinValue(0f)]
    public float Speed = 10f;
    [MinValue(0.0001f)]
    public float Duration = 1f;
    public Vector3 SpawnOffset;
    [MinValue(0f)]
    public float Radius;
    /// <summary>
    /// 子弹可以碰撞的次数，每次碰到目标-1
    /// </summary>
    [MinValue(1)]
    public int HitTimes;
    /// <summary>
    /// 创建后延迟多久才能命中目标
    /// </summary>
    [MinValue(0f)]
    public float CanHitAfterCreated;
    /// <summary>
    /// 碰撞到同一个目标的延迟
    /// </summary>
    [MinValue(0f)]
    public float HitSameDelay;
    /// <summary>
    /// 子弹是否碰到障碍物就消失
    /// </summary>
    public bool RemoveOnObstacle;
    /// <summary>
    /// 是否会命中敌人
    /// </summary>
    public bool HitFoe;
    /// <summary>
    /// 是否会命中友军
    /// </summary>
    public bool HitAlly;

    public BulletAction OnCreate;
    public BulletAction OnHit;
    /// <summary>
    /// 子弹生命周期结束时触发
    /// </summary>
    public BulletAction OnRemoved;

    public bool HasValidLifetime => Duration > 0f;
    public bool HasValidHitTimes => HitTimes > 0;
}
