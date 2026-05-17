using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// 帧末统一销毁所有标记 DestroyTag 的 Entity。
    /// 角色死亡时清理其 BuffContainer。
    /// </summary>
    public class EntityCleanupSystem : ISystem
    {
        private readonly List<Entity> toDestroy = new(32);

        public override void Tick(float delteTime)
        {
            // 先收集，再移除（避免遍历中修改）
            toDestroy.Clear();
            var entities1 = world.Query<TickReadyTagComponent>();
            for (int i = 0; i < entities1.Count; i++)
                toDestroy.Add(entities1[i]);

            for (int i = 0; i < toDestroy.Count; i++)
                world.RemoveComponent<TickReadyTagComponent>(toDestroy[i]);

            toDestroy.Clear();
            var entities = world.Query<DestroyTagComponent>();
            for (int i = 0; i < entities.Count; i++)
            {
                toDestroy.Add(entities[i]);
            }
            for (int i = 0; i < toDestroy.Count; i++)
            {
                var entity = toDestroy[i];
                if (!world.IsAlive(entity)) continue;

                // 如果是角色，清理BuffContainer
                world.DestroyEntity(entity);
            }
        }
    }
}