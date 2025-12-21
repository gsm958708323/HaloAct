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
    /// 理论上这里不应该使用unity相关代码，这里快速实现功能使用unity的API
    /// </summary>
    public class ActorModel : ILogicT<ActorData>
    {
        [HideInInspector]
        public ActorBehaviorComp Behavior
        {
            get { return GetComp<ActorBehaviorComp>(); }
        }
        [HideInInspector]
        public ActorBuffComp Buff
        {
            get { return GetComp<ActorBuffComp>(); }
        }
        [HideInInspector]
        GroundChecker groundChecker
        {
            get
            {
                return gameObject.GetComponent<GroundChecker>();
            }
        }
        [HideInInspector]
        public PlayerGameInput GameInput
        {
            get
            {
                return gameObject.GetComponent<PlayerGameInput>();
            }
        }
        [HideInInspector]
        public HitBox HitBox
        {
            get
            {
                return gameObject.GetComponentInChildren<HitBox>(true);
            }
        }
        [HideInInspector]
        public HurtBox HurtBox
        {
            get
            {
                return gameObject.GetComponentInChildren<HurtBox>(true);
            }
        }

        List<IComponent> compList;
        public int Uid;
        private GameObject gameObject;

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
        public Transform transform
        {
            get
            {
                return gameObject.transform;
            }
        }

        public void Init()
        {

        }

        public T AddComp<T>() where T : IComponent, new()
        {
            var comp = new T();
            compList.Add(comp);

            comp.Init();
            comp.Enter(this);
            return comp;
        }

        public T GetComp<T>() where T : IComponent
        {
            var type = typeof(T);
            for (int i = 0; i < compList.Count; i++)
            {
                if (compList[i].GetType() == type)
                {
                    return (T)compList[i];
                }
            }
            return null;
        }

        public void Enter(ActorData t) { }

        public void Enter(ActorData data, int uid, GameObject actorGo)
        {
            compList = new();
            this.ActorData = data;
            this.Uid = uid;
            this.gameObject = actorGo;

            Debugger.Log(this.HitBox.gameObject.name);

            LoadData();
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