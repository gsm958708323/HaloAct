namespace Combat
{
    public class SourceInfoComponent : IComponent
    {
        public int ConfigId;
        public EntityType Type;
        public Entity Source;
        public Entity Instigator;
    }

    public enum EntityType
    {
        Character,
        Bullet,
        AOE,
        Buff,
    }
}
