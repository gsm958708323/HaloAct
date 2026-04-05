using System.Collections.Generic;
using HaloFrame;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ability.Integration
{
    public class CombatIntegrationTests
    {
        Dispatcher oldDispatcher;
        ConfigManager oldConfig;
        EntityManager oldLogicEntity;
        BulletManager oldBulletManager;
        AoeManager oldAoeManager;
        DamageManager oldDamageManager;

        TestEntityManager testLogicEntity;
        readonly List<Object> createdObjects = new();

        [SetUp]
        public void SetUp()
        {
            oldDispatcher = GameManager.Dispatcher;
            oldConfig = FightManager.Config;
            oldLogicEntity = FightManager.LogicEntity;
            oldBulletManager = FightManager.Bullet;
            oldAoeManager = FightManager.Aoe;
            oldDamageManager = FightManager.Damage;

            GameManager.Dispatcher = new Dispatcher();
            FightManager.Config = new ConfigManager();

            testLogicEntity = new TestEntityManager();
            testLogicEntity.Init();
            FightManager.LogicEntity = testLogicEntity;

            FightManager.Bullet = new BulletManager();
            FightManager.Bullet.Init();

            FightManager.Aoe = new AoeManager();
            FightManager.Aoe.Init();

            FightManager.Damage = new DamageManager();
            FightManager.Damage.Init();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }
            createdObjects.Clear();

            FightManager.LogicEntity?.Destroy();
            FightManager.Bullet?.Destroy();
            FightManager.Aoe?.Destroy();
            FightManager.Damage?.Destroy();

            GameManager.Dispatcher = oldDispatcher;
            FightManager.Config = oldConfig;
            FightManager.LogicEntity = oldLogicEntity;
            FightManager.Bullet = oldBulletManager;
            FightManager.Aoe = oldAoeManager;
            FightManager.Damage = oldDamageManager;
        }

        [Test]
        public void BulletDamage_DeferredBuffAppliesAfterDamageChainAndBeforeAoeTick()
        {
            var trace = new List<string>();
            var attacker = CreateActorEntity(ActorType.PLAYER, attack: 10f);
            var defender = CreateActorEntity(ActorType.Enemy, attack: 0f);
            CreateCombatCollider(defender, new Vector3(0f, 0f, 0.6f), 0.2f);

            var pendingBuff = CreateBuffData(
                5002,
                onOccur: new TraceOccurAction(trace, "buff.applied"),
                attackPlus: 50f);
            var reactionBuff = CreateBuffData(
                5001,
                onBeHurt: new QueueExtraDamageAndPendingBuffAction(trace, pendingBuff));

            defender.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = reactionBuff.Id,
                Data = reactionBuff,
                Creater = attacker,
                Target = defender.Uid,
                AddStack = 1,
                Duration = 5f,
                IsOverrideDuration = true,
            });

            var bulletData = CreateBulletData(6001, new TraceBulletAction(trace, "bullet.hit"));
            FightManager.LogicEntity.CreateBullet(new BulletLauncher
            {
                BulletId = bulletData.Id,
                Data = bulletData,
                Caster = attacker,
                Position = Vector3.zero,
                Direction = Vector3.forward,
            });

            var aoeData = CreateAoeData(7001, new ObserveAttrAoeAction(trace, 40f));
            FightManager.LogicEntity.CreateAoe(new AoeLauncher
            {
                AoeId = aoeData.Id,
                Data = aoeData,
                Caster = attacker,
                Position = Vector3.zero,
                Velocity = Vector3.zero,
            });

            Physics.SyncTransforms();
            RunManagerFrame(1f);

            CollectionAssert.AreEqual(
                new[]
                {
                    "bullet.hit",
                    "damage.resolve.bullet",
                    "damage.resolve.extra.sees_old_state",
                    "buff.applied",
                    "aoe.tick.sees_new_state",
                },
                trace);
        }

        void RunManagerFrame(float deltaTime)
        {
            var managers = new List<OrderedManager>
            {
                new(FightManager.Bullet, 0),
                new(FightManager.Aoe, 1),
                new(FightManager.Damage, 2),
            };

            managers.Sort((left, right) =>
            {
                var priorityCompare = right.Manager.Priority.CompareTo(left.Manager.Priority);
                return priorityCompare != 0 ? priorityCompare : left.Order.CompareTo(right.Order);
            });

            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].Manager.Tick(deltaTime);
            }
        }

        Entity CreateActorEntity(ActorType actorType, float attack)
        {
            var actorData = ScriptableObject.CreateInstance<ActorData>();
            actorData.ActorType = actorType;
            actorData.BaseAttr = new ActorAttr
            {
                Attack = attack,
            };
            actorData.BaseControlState = ActorControlState.CreateDefault();
            createdObjects.Add(actorData);
            return testLogicEntity.CreateTestActor(actorData);
        }

        GameObject CreateCombatCollider(Entity entity, Vector3 position, float radius)
        {
            var go = new GameObject($"CombatTarget-{entity.Uid}");
            go.transform.position = position;

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = false;
            collider.radius = radius;

            go.AddComponent<HurtBox>();
            var idCard = go.AddComponent<IdentitCard>();
            idCard.Uid = entity.Uid;

            createdObjects.Add(go);
            return go;
        }

        BuffData CreateBuffData(int id, BuffBeHurtAction onBeHurt = null, BuffOccurAction onOccur = null, float attackPlus = 0f)
        {
            var data = ScriptableObject.CreateInstance<BuffData>();
            data.Id = id;
            data.MaxStack = 1;
            data.Modifier = new BuffModifierGroup
            {
                BuffPlus = new ActorAttr
                {
                    Attack = attackPlus,
                },
                ControlStateMod = ActorControlState.CreateDefault(),
            };
            data.OnBeHurt = onBeHurt;
            data.OnOccur = onOccur;
            createdObjects.Add(data);
            return data;
        }

        BulletData CreateBulletData(int id, BulletAction onHit)
        {
            var data = ScriptableObject.CreateInstance<BulletData>();
            data.Id = id;
            data.Duration = 1f;
            data.Speed = 1f;
            data.Radius = 0.1f;
            data.HitTimes = 1;
            data.HitFoe = true;
            data.HitAlly = false;
            data.OnHit = onHit;
            createdObjects.Add(data);
            return data;
        }

        AoeData CreateAoeData(int id, AoeAction onTick)
        {
            var data = ScriptableObject.CreateInstance<AoeData>();
            data.Id = id;
            data.Radius = 1f;
            data.Duration = 1f;
            data.TickInterval = 0f;
            data.HitFoe = true;
            data.HitAlly = false;
            data.AffectActors = true;
            data.OnTick = onTick;
            createdObjects.Add(data);
            return data;
        }

        readonly struct OrderedManager
        {
            public OrderedManager(HaloFrame.IManager manager, int order)
            {
                Manager = manager;
                Order = order;
            }

            public HaloFrame.IManager Manager { get; }
            public int Order { get; }
        }

        class TestEntityManager : EntityManager
        {
            public Entity CreateTestActor(ActorData data)
            {
                var entity = AddEntity(EntityType.Actor);
                var dataComp = entity.AddComp<PlayerDataComp>();
                dataComp.Data = data;
                entity.AddComp<AttrComp>();
                entity.AddComp<EffectComp>();
                entity.AddComp<AttackComp>();
                return entity;
            }
        }

        class TraceBulletAction : BulletAction
        {
            readonly List<string> trace;
            readonly string label;

            public TraceBulletAction(List<string> trace, string label)
            {
                this.trace = trace;
                this.label = label;
            }

            protected override void OnExecute()
            {
                trace.Add(label);
            }
        }

        class TraceOccurAction : BuffOccurAction
        {
            readonly List<string> trace;
            readonly string label;

            public TraceOccurAction(List<string> trace, string label)
            {
                this.trace = trace;
                this.label = label;
            }

            protected override void OnExecute()
            {
                trace.Add(label);
            }
        }

        class QueueExtraDamageAndPendingBuffAction : BuffBeHurtAction
        {
            readonly List<string> trace;
            readonly BuffData pendingBuff;

            public QueueExtraDamageAndPendingBuffAction(List<string> trace, BuffData pendingBuff)
            {
                this.trace = trace;
                this.pendingBuff = pendingBuff;
            }

            protected override void OnExecute()
            {
                if (damage.HasTag(DamageTag.Bullet))
                {
                    trace.Add("damage.resolve.bullet");
                    damage.PendingBuffs ??= new List<AddBuffInfo>();
                    damage.PendingBuffs.Add(new AddBuffInfo
                    {
                        BuffId = pendingBuff.Id,
                        Data = pendingBuff,
                        Creater = damage.Attacker,
                        Target = damage.Defender.Uid,
                        AddStack = 1,
                        Duration = 5f,
                        IsOverrideDuration = true,
                    });

                    FightManager.Damage.Enqueue(new DamageInfo
                    {
                        Attacker = damage.Attacker,
                        Defender = damage.Defender,
                        Source = this,
                        Tags = DamageTag.Extra,
                        TriggerHurtBehavior = false,
                    });
                    return;
                }

                if (!damage.HasTag(DamageTag.Extra))
                {
                    return;
                }

                var attack = damage.Defender.GetComp<AttrComp>()?.FinalAttr.Attack ?? 0f;
                trace.Add(attack > 0f
                    ? "damage.resolve.extra.sees_new_state"
                    : "damage.resolve.extra.sees_old_state");
            }
        }

        class ObserveAttrAoeAction : AoeAction
        {
            readonly List<string> trace;
            readonly float threshold;

            public ObserveAttrAoeAction(List<string> trace, float threshold)
            {
                this.trace = trace;
                this.threshold = threshold;
            }

            protected override void OnExecute()
            {
                var attack = target?.GetComp<AttrComp>()?.FinalAttr.Attack ?? 0f;
                trace.Add(attack >= threshold
                    ? "aoe.tick.sees_new_state"
                    : "aoe.tick.sees_old_state");
            }
        }
    }
}
