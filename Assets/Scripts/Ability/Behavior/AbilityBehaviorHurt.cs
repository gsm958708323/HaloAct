using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 负责受击的行为
    /// </summary>
    [CreateAssetMenu(fileName = "NewBehavior", menuName = "AbilityTree/HurtBehavior")]
    public class AbilityBehaviorHurt : AbilityBehavior
    {
        /// <summary>
        /// 受伤事件
        /// </summary>
        /// <returns></returns>
        public List<AbilityUnderAttack> HurtEvents = new();

        public AttackType AttackType;
    }
}
