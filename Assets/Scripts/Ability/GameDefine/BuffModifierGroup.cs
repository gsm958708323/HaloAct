using System;

namespace Ability
{
    [Serializable]
    public struct BuffModifierGroup
    {
        public ActorAttr BuffPlus;
        public ActorAttr BuffRatio;
        public ActorControlState ControlStateMod;

        public static BuffModifierGroup CreateDefault()
        {
            return new BuffModifierGroup
            {
                ControlStateMod = ActorControlState.CreateDefault(),
            };
        }

        public ActorAttr GetStackedPlus(int stack)
        {
            return Scale(BuffPlus, stack);
        }

        public ActorAttr GetStackedRatio(int stack)
        {
            return Scale(BuffRatio, stack);
        }

        static ActorAttr Scale(ActorAttr attr, int stack)
        {
            var factor = Math.Max(0, stack);
            return new ActorAttr
            {
                Hp = attr.Hp * factor,
                MaxHp = attr.MaxHp * factor,
                Attack = attr.Attack * factor,
                Defense = attr.Defense * factor,
                MoveSpeed = attr.MoveSpeed * factor,
                Gravity = attr.Gravity * factor,
                DelayAerialTime = attr.DelayAerialTime * factor,
            };
        }
    }
}
