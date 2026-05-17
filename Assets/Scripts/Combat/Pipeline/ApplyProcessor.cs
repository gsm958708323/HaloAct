using System;
using System.Net;
using UnityEngine;

namespace Combat
{
    public class ApplyProcessor : IEffectProcessor
    {
        private readonly IEventBus eventBus;
        private readonly BuffFactory buffFactory;
        private readonly BulletFactory bulletFactory;
        private readonly AoeFactory aoeFactory;


        public ApplyProcessor(
            IEventBus eventBus,
            BuffFactory buffFactory,
            BulletFactory bulletFactory,
            AoeFactory aoeFactory)
        {
            this.eventBus = eventBus;
            this.buffFactory = buffFactory;
            this.bulletFactory = bulletFactory;
            this.aoeFactory = aoeFactory;
        }

        public string Name => "执行效果";

        public void Process(EffectRequest request, World world)
        {
            if (request.Cancelled || request.Absorbed) return;
            switch (request.Type)
            {
                case EffectType.Damage:
                    ProcessDamage(request, world);
                    break;

                case EffectType.Heal:
                    ProcessHeal(request, world);
                    break;

                case EffectType.ApplyBuff:
                    ProcessApplyBuff(request, world);
                    break;

                case EffectType.RemoveBuff:
                    ProcessRemoveBuff(request, world);
                    break;

                case EffectType.SpawnAOE:
                    ProcessSpawnAOE(request, world);
                    break;

                case EffectType.SpawnBullet:
                    ProcessSpawnBullet(request, world);
                    break;

                case EffectType.Dispel:
                    ProcessDispel(request, world);
                    break;
            }
        }

        private void ProcessDamage(EffectRequest request, World world)
        {
            if (!world.IsAlive(request.Target)) return;

            var health = world.GetComponent<HealthComponent>(request.Target);
            if (health == null || health.IsDead) return;

            health.Current -= request.Value;
            eventBus.Publish(new DamageEvent
            {
                Source = request.Source,
                Target = request.Target,
                Value = request.Value,
                DamageType = request.DamageType,
            });

            if (health.Current <= 0)
            {
                health.Current = 0;
                health.IsDead = true;

                eventBus.Publish(new EntityDeathEvent
                {
                    Entity = request.Target,
                    Killer = request.Source,
                });
            }
        }

        private void ProcessHeal(EffectRequest request, World world)
        {
            if (!world.IsAlive(request.Target)) return;

            var health = world.GetComponent<HealthComponent>(request.Target);
            if (health == null || health.IsDead) return;
            health.Current = Mathf.Min(
                health.Current + request.Value, health.Max);

            eventBus.Publish(new HealEvent
            {
                Source = request.Source,
                Target = request.Target,
                Value = request.Value,
            });
        }

        private void ProcessApplyBuff(EffectRequest request, World world)
        {
            if (request.Target.IsNull || !world.IsAlive(request.Target)) return;

            buffFactory.ApplyBuff(
                request.Target,
                request.ReferenceId,
                request.Source);
        }

        private void ProcessRemoveBuff(EffectRequest request, World world)
        {
            if (request.Target.IsNull || !world.IsAlive(request.Target)) return;

            buffFactory.RemoveBuffByConfigId(
                request.Target,
                request.ReferenceId,
                BuffRemovalReason.Manual);
        }

        private void ProcessSpawnAOE(EffectRequest request, World world)
        {
            Vector3 pos = request.HitPoint;
            Vector3 dir = request.Direction;

            // 如果没有指定命中点，用 Instigator 或 Source 的位置
            if (pos == Vector3.zero && !request.Instigator.IsNull
                && world.IsAlive(request.Instigator))
            {
                var tr = world.GetComponent<TransformComponent>(
                    request.Instigator);
                if (tr != null)
                {
                    pos = tr.Position;
                    dir = tr.Forward;
                }
            }

            if (pos == Vector3.zero && !request.Source.IsNull
                && world.IsAlive(request.Source))
            {
                var tr = world.GetComponent<TransformComponent>(request.Source);
                if (tr != null)
                {
                    pos = tr.Position;
                    dir = tr.Forward;
                }
            }

            aoeFactory.Create(
                request.ReferenceId,
                request.Source,
                request.Instigator,
                pos, dir);
        }

        private void ProcessSpawnBullet(EffectRequest request, World world)
        {
             Vector3 pos = request.HitPoint;
            Vector3 dir = request.Direction;

            if (pos == Vector3.zero && !request.Source.IsNull
                && world.IsAlive(request.Source))
            {
                var tr = world.GetComponent<TransformComponent>(request.Source);
                if (tr != null)
                {
                    pos = tr.Position;
                    dir = tr.Forward;
                }
            }

            bulletFactory.Create(
                request.ReferenceId,
                request.Source,
                pos, dir);
        }

        private void ProcessDispel(EffectRequest request, World world)
        {
            throw new NotImplementedException();
        }
    }

    public struct HealEvent
    {
        public Entity Source { get; set; }
        public Entity Target { get; set; }
        public float Value { get; set; }
    }

    internal struct EntityDeathEvent
    {
        public Entity Entity { get; set; }
        public Entity Killer { get; set; }
    }

    internal struct DamageEvent
    {
        public Entity Source { get; set; }
        public Entity Target { get; set; }
        public float Value { get; set; }
        public DamageType DamageType { get; set; }
    }
}