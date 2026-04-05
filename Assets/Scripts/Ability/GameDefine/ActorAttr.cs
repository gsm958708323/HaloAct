using System;

namespace Ability
{
    [Serializable]
    public struct ActorAttr
    {
        public float Hp;
        public float MaxHp;
        public float Attack;
        public float Defense;
        public float MoveSpeed;
        public float Gravity;
        public float DelayAerialTime;

        public static ActorAttr operator +(ActorAttr left, ActorAttr right)
        {
            return new ActorAttr
            {
                Hp = left.Hp + right.Hp,
                MaxHp = left.MaxHp + right.MaxHp,
                Attack = left.Attack + right.Attack,
                Defense = left.Defense + right.Defense,
                MoveSpeed = left.MoveSpeed + right.MoveSpeed,
                Gravity = left.Gravity + right.Gravity,
                DelayAerialTime = left.DelayAerialTime + right.DelayAerialTime,
            };
        }

        public static ActorAttr LerpRatio(ActorAttr baseAttr, ActorAttr ratio)
        {
            return new ActorAttr
            {
                Hp = baseAttr.Hp * (1f + ratio.Hp),
                MaxHp = baseAttr.MaxHp * (1f + ratio.MaxHp),
                Attack = baseAttr.Attack * (1f + ratio.Attack),
                Defense = baseAttr.Defense * (1f + ratio.Defense),
                MoveSpeed = baseAttr.MoveSpeed * (1f + ratio.MoveSpeed),
                Gravity = baseAttr.Gravity * (1f + ratio.Gravity),
                DelayAerialTime = baseAttr.DelayAerialTime * (1f + ratio.DelayAerialTime),
            };
        }
    }
}
