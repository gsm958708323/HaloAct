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

        [FolderPath] public string NodePath;
        [FolderPath] public string BehaviorPath;
        public GroundCheckData CheckerData;

    }
}