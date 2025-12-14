using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        [HideInInspector] public ActorBehaviorComp Behavior;
        [HideInInspector] public ActorBuffComp Buff;
        [HideInInspector] GroundChecker groundChecker;
        [HideInInspector] public PlayerGameInput GameInput;
        [HideInInspector] public HitBox HitBox;
        [HideInInspector] public HurtBox HurtBox;
        List<ILogicT<ActorModel>> compList;

        /// <summary>
        /// 创建者
        /// </summary>
        public ActorModel Creater
        {
            get { return creater; }
            set
            {
                creater = value;
                TryFollowOwner();
            }
        }
        ActorModel creater;
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

        T AddComp<T>(T comp) where T : ILogicT<ActorModel>
        {
            compList.Add(comp);
            return comp;
        }

        void InitComp()
        {
            compList = new();

            Behavior = AddComp(new ActorBehaviorComp());
            Behavior.Init(ActorData.NodePath, ActorData.BehaviorPath);
            Behavior.Enter(this);

            Buff = AddComp(new ActorBuffComp());
            Buff.Init();
            Buff.Enter(this);
        }

        public void Enter(ActorData actorData)
        {
            ActorData = actorData;
            LoadData();

            InitComp();
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
                if (Creater == null)
                {
                    Debugger.LogError($"[出生位置] 跟随创建者时获取Owner失败 {ActorData.Id}", LogDomain.Actor);
                    return;
                }
                Position = Creater.Position + bornInfo.Pos;
                Rotation = Creater.Rotation;
            }
        }

        public void Tick(float deltaTime)
        {
            foreach (var item in compList)
            {
                item.Tick(deltaTime);
            }

            UpdateVelocity();
            CheckGround();
        }

        public void Exit()
        {
            foreach (var item in compList)
            {
                item.Exit();
            }
            compList = null;

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