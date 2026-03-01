using System.Collections;
using System.Collections.Generic;
using Ability;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBullet", menuName = "AbilityTree/BulletData")]
public class BulletData : SerializedScriptableObject
{
    public int Id;
    public GameObject Prefab;

    [BoxGroup("Movement")]
    public float Speed = 10f;

    [BoxGroup("Movement")]
    public float Duration = 2f;

    [BoxGroup("Movement")]
    public Vector3 SpawnOffset = new Vector3(0, 1, 0.8f);

    public float Radius;

    [BoxGroup("Hit")]
    public AttackType AttackType = AttackType.Normal;
    /// <summary>
    /// 子弹可以碰撞的次数，每次碰到目标-1
    /// </summary>
    public int HitTimes;
    /// <summary>
    /// 碰撞到同一个目标的延迟
    /// </summary>
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
    
}
