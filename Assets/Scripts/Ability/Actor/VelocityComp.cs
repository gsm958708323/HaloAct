using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class VelocityComp : ComponentLogic
    {
        /// <summary>
        /// 外部设置方向时调用
        /// </summary>
        public Vector3 Velocity;
        public float DelayAerialTime { get; set; }
        public float Gravity { get; set; }
        public Vector3 Frictional { get; set; }

        private bool IsGround;
        private bool IsAerial;
        private float aerialTime;

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            UpdateVelocity(deltaTime);
            CheckGround(deltaTime);
        }

        private void CheckGround(float deltaTime)
        {
            IsGround = UnityGameAPI.CheckGround(entity.Uid);
            if (IsGround)
            {
                Velocity.y = 0;
                IsAerial = false;
                aerialTime = 0;
            }
            else
            {
                // 延迟一段时间后才算空中
                if (!IsAerial)
                {
                    aerialTime += deltaTime;
                }

                if (aerialTime > DelayAerialTime)
                {
                    IsAerial = true;
                }
            }
        }

        private void UpdateVelocity(float deltaTime)
        {
            var transComp = entity.GetComp<TransfromComp>();
            if (transComp is null)
            {
                return;
            }
            transComp.Position += Velocity * deltaTime;

            if (!Mathf.Approximately(Gravity, 0))
            {
                Velocity.y += Gravity * deltaTime;
                Velocity.y = Mathf.Clamp(Velocity.y, -20, 20);
            }

            // 用来处理速度的衰减，速度不断变小并无限接近0
            Velocity.Scale(Frictional);
        }
    }
}