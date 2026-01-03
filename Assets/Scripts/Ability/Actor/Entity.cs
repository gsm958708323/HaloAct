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
    public class Entity : IEntity
    {
        Entity creater;
        /// <summary>
        /// 目标
        /// </summary>
        public Entity Target;

        public bool IsDead;
        public bool IsInvincible;

        internal void DeathCheck()
        {

        }
    }
}