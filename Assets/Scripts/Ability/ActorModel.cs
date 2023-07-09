using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using Sirenix.OdinInspector;
using System;
using MovementSystem;

namespace Ability
{
    public enum ActorType
    {
        PLAYER,
        Enemy,
    }

    public class ActorModel : MonoBehaviour
    {
        [FolderPath] public string NodePath;
        [FolderPath] public string BehaviorPath;
        [HideInInspector] public AbilityBehaviorTree tree;
        [HideInInspector] public HitBox HitBox;
        [HideInInspector] public HurtBox HurtBox;
        public ActorType ActorType;
        public ActorModel Target;

        /// <summary>
        /// 缓存时间，用于计算帧数
        /// </summary>
        private float cacheTime;
        /// <summary>
        /// 当前运行的帧数
        /// </summary>
        private int curFrame;
        private float fps;

        public bool IsDead { get; internal set; }
        public bool IsInvincible { get; internal set; }

        private void Awake()
        {
            fps = 1.0f / GameManager_Settings.TargetFraneRate;
            curFrame = 1;
        }

        void Start()
        {
            tree = new AbilityBehaviorTree();
            tree.Init(NodePath, BehaviorPath);
            tree.Enter(this);

            HitBox = GetComponentInChildren<HitBox>();
            HitBox.Init();
            HitBox.Exit(); // 默认隐藏

            HurtBox = GetComponentInChildren<HurtBox>();
            HurtBox.Init();
            HurtBox.Enter(this);


        }

        void Update()
        {
            cacheTime += Time.deltaTime;

            // 超过fps执行一次Tick
            while (cacheTime > fps)
            {
                tree.Tick(curFrame);
                curFrame += 1;
                cacheTime -= fps;
            }
        }

        // internal AbilityAttack GetCurAbilityAttack()
        // {
        //     if (GetCurAbilityBehavior() is AbilityBehaviorAttack attackBehavior)
        //     {
        //         return attackBehavior.CurAttack;
        //     }
        //     return null;
        // }

        // internal AbilityBehavior GetCurAbilityBehavior()
        // {
        //     return tree?.curNode?.Behavior;
        // }

        internal void DeathCheck()
        {

        }
    }
}