using System;
using ParadoxNotion;

namespace Combat
{
    /// <summary>
    /// entity为值类型，相当于一个唯一索引来，可以直接new
    /// </summary>
    public struct Entity
    {
        /// <summary>
        /// 唯一id，回池之后可复用
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// 回池之后版本号+1
        /// </summary>
        public readonly int Version;
        public Entity(int id, int version)
        {
            Id = id;
            Version = version;
        }

        public static readonly Entity Null = new Entity(0, 0);
        public bool IsNull => Id == 0 && Version == 0;
        public bool Equals(Entity other)
        {
            return Id == other.Id && Version == other.Version;
        }
        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Version);
        }
        public static bool operator ==(Entity a, Entity b) => a.Equals(b);
        public static bool operator !=(Entity a, Entity b) => !a.Equals(b);

        public override string ToString() => $"Entity({Id}:{Version})";
    }
}

