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
        Bullet,
    }

    /// <summary>
    /// 理论上这个代码不应该继承与mono，这里继承是因为有些检测逻辑是用的unity的API
    /// </summary>
    public class ActorModel : MonoBehaviour, ILogicT<ActorData>
    {
        [HideInInspector] public AbilityBehaviorTree tree;
        [HideInInspector] GroundChecker groundChecker;
        [HideInInspector] public PlayerGameInput GameInput;
        [HideInInspector] public HitBox HitBox;
        [HideInInspector] public HurtBox HurtBox;

        /// <summary>
        /// 创建者
        /// </summary>
        public ActorModel Owner
        {
            get { return owner; }
            set
            {
                owner = value;
                TryFollowOwner();
            }
        }
        ActorModel owner;
        /// <summary>
        /// 目标
        /// </summary>
        public ActorModel Target;

        public bool IsDead;
        public bool IsInvincible;
        public bool IsGround;
        public bool IsAerial;
        float cacheAerialTime;
        public Quaternion Rotation;
        /// <summary>
        /// 外部不能直接设置位置，通过设置Velocity来改变位置 
        /// </summary>
        public Vector3 Position { private set; get; }
        /// <summary>
        /// 外部设置方向时调用
        /// </summary>
        public Vector3 Velocity;
        public ActorData ActorData;

        public void Init()
        {

        }

        public void Enter(ActorData actorData)
        {
            ActorData = actorData;
            LoadData();

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

        void LoadData()
        {
            var bornInfo = ActorData.BornPosInfo;
            if (bornInfo.BornPosEnum == BornPosEnum.FixedPosition)
            {
                Position = bornInfo.Pos;
            }
        }

        void TryFollowOwner()
        {
            var bornInfo = ActorData.BornPosInfo;
            if (bornInfo.BornPosEnum == BornPosEnum.FollowOwner)
            {
                if (Owner == null)
                {
                    Debugger.LogError($"[出生位置] 跟随创建者时获取Owner失败 {ActorData.Id}", LogDomain.Actor);
                    return;
                }
                Position = Owner.Position + bornInfo.Pos;
            }
        }

        public void Tick(float deltaTime)
        {
            tree.Tick(deltaTime);

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