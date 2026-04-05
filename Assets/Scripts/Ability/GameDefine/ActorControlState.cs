using System;

namespace Ability
{
    [Serializable]
    public struct ActorControlState
    {
        public bool CanMove;
        public bool CanRotate;
        public bool CanUseSkill;
        public bool CanAttack;
        public bool CanBeControlled;

        public static ActorControlState CreateDefault()
        {
            return new ActorControlState
            {
                CanMove = true,
                CanRotate = true,
                CanUseSkill = true,
                CanAttack = true,
                CanBeControlled = true,
            };
        }
    }
}
