using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using Sirenix.OdinInspector;

public class ActorModel : MonoBehaviour
{
    public AbilityBehaviorTree tree;
    [FolderPath] public string NodePath;
    [FolderPath] public string BehaviorPath;

    // Start is called before the first frame update
    void Start()
    {
        tree = new AbilityBehaviorTree(this);
        tree.Init(NodePath, BehaviorPath);
    }

    // Update is called once per frame
    void Update()
    {
        tree.Tick();
    }
}
