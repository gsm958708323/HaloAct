using System;

namespace Ability
{
    [Flags]
    public enum DamageTag
    {
        None = 0,
        Melee = 1 << 0,
        Bullet = 1 << 1,
        Extra = 1 << 2,
        Buff = 1 << 3,
        Aoe = 1 << 4,
    }
}
