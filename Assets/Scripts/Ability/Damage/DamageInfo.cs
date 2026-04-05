using System.Collections.Generic;

namespace Ability
{
    public class DamageInfo
    {
        public Entity Attacker;
        public Entity Defender;
        public object Source;
        public DamageTag Tags;
        public List<AddBuffInfo> PendingBuffs;
        public bool TriggerHurtBehavior = true;
        public bool IsLethal;

        public bool HasTag(DamageTag tag)
        {
            return (Tags & tag) != 0;
        }
    }
}
