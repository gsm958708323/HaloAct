using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class TransfromComp : IComponent
    {
        public bool IsGround;
        public bool IsAerial;
        float cacheAerialTime;

        /// <summary>
        /// 外部不能直接设置位置，通过设置Velocity来改变位置 
        /// </summary>
        public Vector3 Position { private set; get; }
        public Quaternion Rotation;
        /// <summary>
        /// 外部设置方向时调用
        /// </summary>
        public Vector3 Velocity;
        public Vector3 forward
        {
            get
            {
                return Rotation * Vector3.forward;
            }
        }
        public Vector3 right
        {
            get
            {
                return Rotation * Vector3.right;
            }
        }
        public Vector3 up
        {
            get
            {
                return Rotation * Vector3.up;
            }
        }


        private GameObject gameObject;
        private ActorData data;

        GroundChecker groundChecker
        {
            get
            {
                return gameObject.GetComponent<GroundChecker>();
            }
        }

        public override void Enter(ActorModel actor)
        {
            gameObject = actor.gameObject;
            data = actor.GetComp<ActorDataComp>().Data;

            var bornInfo = data.BornPosInfo;
            if (bornInfo.BornPosEnum == BornPosEnum.FixedPosition)
            {
                Position = bornInfo.Pos;
            }
        }

        public override void Tick(float deltaTime)
        {
            UpdateVelocity();
            CheckGround();
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

                if (cacheAerialTime > data.DelayAerialTime)
                {
                    IsAerial = true;
                }
            }
        }

        private void UpdateVelocity()
        {
            Position += Velocity * GameManager.Instance.FrameInterval;

            Velocity.y += data.Gravity * GameManager.Instance.FrameInterval;
            Velocity.y = Mathf.Clamp(Velocity.y, -20, 20);

            // 用来处理速度的衰减，速度不断变小并无限接近0
            Velocity.Scale(data.Frictional);
        }
    }
}
