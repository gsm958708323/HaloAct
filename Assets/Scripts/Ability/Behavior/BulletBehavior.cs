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
    public class BulletBehavior : BehaviorBase
    {
        public BulletAction OnAddAction;
        public BulletAction OnHitAction;
        public BulletAction OnRemoveAction;
    }
}
