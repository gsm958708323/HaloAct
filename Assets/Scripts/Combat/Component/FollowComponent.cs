using UnityEngine;

namespace Combat
{
    public class FollowComponent : IComponent
    {
        public Entity Target;
        public Vector3 Offset;
        public bool InheritRotation;    // 是否继承朝向
        public bool DestroyOnTargetDead;// 目标死亡时是否自毁
    }
}
