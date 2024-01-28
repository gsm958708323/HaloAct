using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 负责攻击的行为
    /// </summary>
    [CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/Attackehavior")]
    public class AbilityBehaviorAttack : AbilityBehavior
    {
        /// <summary>
        /// 攻击列表
        /// </summary>
        /// <returns></returns>
        public List<AbilityAttack> Attacks = new();
        [HideInInspector] public AbilityAttack CurAttack;
        /// <summary>
        /// 格挡事件
        /// </summary>
        /// <returns></returns>
        public List<AbilityUnderAttack> BlockEvents = new();

        public override void Tick(int frame)
        {
            base.Tick(frame);
            UpdateAttack(frame);
        }

        public override void Exit()
        {
            foreach (var attack in Attacks)
            {
                if (attack.IsEnter())
                {
                    attack.Exit();
                }
            }

            base.Exit();
        }

        private void UpdateAttack(int curFrame)
        {
            foreach (var attack in Attacks)
            {
                int startFrame = attack.FrameInfo.StartFrame;
                int endFrame = attack.FrameInfo.EndFrame;

                if (curFrame == startFrame)
                {
                    CurAttack = attack;
                    attack.Enter(tree.ActorModel);
                }

                if (curFrame >= startFrame && curFrame <= endFrame)
                {
                    attack.Tick(curFrame);
                }

                if (curFrame == endFrame)
                {
                    attack.Exit();
                    CurAttack = null;
                }
            }
        }

    }
}
