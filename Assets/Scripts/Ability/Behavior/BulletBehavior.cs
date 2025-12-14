using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 负责子弹的行为
    /// </summary>
    [CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/BulletBehavior")]
    public class BulletBehavior : AbilityBehaviorAttack
    {
        public float Lifetime;
        public AbilitySimpleAction OnAddAction;
        public AbilitySimpleAction OnHitAction;
        public AbilitySimpleAction OnRemoveAction;

        public override void Init()
        {
            base.Init();
            FrameLength = (int)Math.Ceiling(Lifetime * GameManager.Instance.TargetFrameRate);
        }

        public override void Enter(ActorBehaviorComp tree)
        {
            base.Enter(tree);
            // OnAddAction?.Enter(tree);
        }

        public override void Exit()
        {
            // OnRemoveAction?.Enter(tree);
            base.Exit();
        }
    }
}