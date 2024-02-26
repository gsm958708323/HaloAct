using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Ability
{
    [CreateAssetMenu(fileName = "NewActor", menuName = "AbilityTree/NewActor")]
    public class ActorData : SerializedScriptableObject
    {
        public int Id;
        public ActorType ActorType;
        public GameObject Prefab;
        public BornPosInfo BornPosInfo;
        public Vector3 Frictional = new Vector3(0.5f, 1, 0.5f);
        public float Gravity = -20f;
        public float DelayAerialTime = 0.5f;

        // todo 改成用id获取路径
        [FolderPath] public string NodePath;
        [FolderPath] public string BehaviorPath;
        public GroundCheckData CheckerData;
    }

    [Serializable]
    public class BornPosInfo
    {
        public BornPosEnum BornPosEnum;
        public Vector3 Pos;
    }

    public enum BornPosEnum
    {
        FixedPosition,
        FollowOwner,
    }
}