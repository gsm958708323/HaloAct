using UnityEngine;

namespace Combat
{
    public class FollowSystem : ISystem
    {
        public override void Tick(float delteTime)
        {
            var entities = world.Query<FollowComponent, TransformComponent>();
            for (int i = 0; i < entities.Count - 1; i++)
            {
                var entity = entities[i];
                var follow = world.GetComponent<FollowComponent>(entity);
                var transform = world.GetComponent<TransformComponent>(entity);
                if (!world.IsAlive(follow.Target))
                {
                    if (follow.DestroyOnTargetDead)
                    {
                        world.AddComponent<DestroyTagComponent>(entity);
                    }
                    else
                    {
                        world.RemoveComponent<FollowComponent>(entity);
                    }
                    continue;
                }

                var targetTransform = world.GetComponent<TransformComponent>(follow.Target);
                if (targetTransform == null) continue;

                transform.Position = targetTransform.Position + follow.Offset;
                if (follow.InheritRotation)
                    transform.Forward = targetTransform.Forward;

                var collider = world.GetComponent<ColliderComponent>(entity);
                if (collider != null)
                {
                    collider.Data.Center = transform.Position;
                    if (follow.InheritRotation)
                    {
                        collider.Data.Rotation = Quaternion.LookRotation(transform.Forward);
                    }
                }
            }
        }
    }
}