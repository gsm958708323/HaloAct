using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Ability
{
    public enum ActorType
    {
        PLAYER,
        Enemy,
    }

    /// <summary>
    /// 理论上这个代码不应该继承与mono，这里继承是因为有些检测逻辑是用的unity的API
    /// </summary>
    public class ActorModel : MonoBehaviour, ILogicT<ActorData>
    {
        public AbilityBehaviorTree tree;
        [HideInInspector] GroundChecker groundChecker;
        [HideInInspector] public PlayerGameInput GameInput;
        [HideInInspector] public HitBox HitBox;
        [HideInInspector] public HurtBox HurtBox;

        public ActorType ActorType;
        public ActorModel Target;

        public bool IsDead;
        public bool IsInvincible;
        public bool IsGround;
        public bool IsAerial;
        float cacheAerialTime;

        public Vector2 InputDir;
        public Quaternion Rotation;
        /// <summary>
        /// 外部不能直接设置位置，通过设置Velocity来改变位置 
        /// </summary>
        public Vector3 Position { private set; get; }
        /// <summary>
        /// 外部设置方向时调用
        /// </summary>
        public Vector3 Velocity;

        private int id;
        public ActorData ActorData;

        public void Init()
        {

        }

        public void Enter(ActorData actorData)
        {
            ActorData = actorData;
            Position = actorData.BornPos;
            tree = new AbilityBehaviorTree();
            tree.Init(actorData.NodePath, actorData.BehaviorPath);
            tree.Enter(this);

            HitBox = GetComponentInChildren<HitBox>();
            HitBox.Init();
            HitBox.Exit(); // 默认隐藏

            HurtBox = GetComponentInChildren<HurtBox>();
            HurtBox.Init();
            HurtBox.Enter(this);

            GameInput = gameObject.AddComponent<PlayerGameInput>();
            groundChecker = gameObject.AddComponent<GroundChecker>();
            groundChecker.Init(actorData.CheckerData, this);
        }

        public void Tick(int frame)
        {
            tree.Tick(frame);

            UpdateVelocity();
            CheckGround();
        }

        public void Exit()
        {
            tree.Exit();
            tree = null;

            ActorData = null;
        }

        private void CheckGround()
        {
            IsGround = groundChecker.CheckGround();
            if (IsGround)
            {
                Velocity.y = 0;
                IsAerial = false;
                cacheAerialTime = 0;
            }
            else
            {
                // 延迟一段时间后才算空中
                if (!IsAerial)
                {
                    cacheAerialTime += GameManager.Instance.FrameInterval;
                }

                if (cacheAerialTime > ActorData.DelayAerialTime)
                {
                    IsAerial = true;
                }
            }
        }

        private void UpdateVelocity()
        {
            Position += Velocity * GameManager.Instance.FrameInterval;

            Velocity.y += ActorData.Gravity * GameManager.Instance.FrameInterval;
            Velocity.y = Mathf.Clamp(Velocity.y, -20, 20);

            // 用来处理速度的衰减，速度不断变小并无限接近0
            Velocity.Scale(ActorData.Frictional);
        }

        internal void DeathCheck()
        {

        }


    }
}