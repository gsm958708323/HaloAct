using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Ability
{
    [CreateAssetMenu(fileName = "NewActor", menuName = "AbilityTree/NewActor")]
    public class ActorData : SerializedScriptableObject
    {
        public int id;
        public ActorType ActorType;
        public GameObject Prefab;
        public Vector3 BornPos;
        public Vector3 Frictional = new Vector3(0.5f, 1, 0.5f);
        public float Gravity = -20f;
        public float DelayAerialTime = 0.5f;

        [FolderPath] public string NodePath;
        [FolderPath] public string BehaviorPath;
        public GroundCheckData CheckerData;

    }
}