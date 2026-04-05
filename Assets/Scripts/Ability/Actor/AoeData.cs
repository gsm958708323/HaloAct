using Ability;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAoe", menuName = "AbilityTree/AoeData")]
public class AoeData : SerializedScriptableObject
{
    [MinValue(1)]
    public int Id;
    public GameObject Prefab;
    [MinValue(0.0001f)]
    public float Radius = 1f;
    [MinValue(0.0001f)]
    public float Duration = 1f;
    [MinValue(0f)]
    public float TickInterval;
    public bool AffectActors = true;
    public bool AffectBullets;
    public bool HitFoe = true;
    public bool HitAlly;
    public Ability.AoeMoveInfo MoveInfo = Ability.AoeMoveInfo.CreateDefault();

    public Ability.AoeAction OnCreate;
    public Ability.AoeAction OnEnter;
    public Ability.AoeAction OnTick;
    public Ability.AoeAction OnLeave;
    public Ability.AoeAction OnRemoved;

    public bool HasTimedTick => TickInterval > 0f;
}
