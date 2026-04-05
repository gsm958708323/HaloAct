using System.Collections.Generic;
using HaloFrame;
using NUnit.Framework;
using UnityEngine;
using Ability;

namespace Ability.Damage
{
    public class DamageManagerTests
    {
        Dispatcher oldDispatcher;
        ConfigManager oldConfig;
        EntityManager oldLogicEntity;
        DamageManager oldDamageManager;
        int uidSeed;

        [SetUp]
        public void SetUp()
        {
            oldDispatcher = GameManager.Dispatcher;
            oldConfig = FightManager.Config;
            oldLogicEntity = FightManager.LogicEntity;
            oldDamageManager = FightManager.Damage;

            GameManager.Dispatcher = new Dispatcher();
            FightManager.Config = new ConfigManager();
            FightManager.LogicEntity = new EntityManager();
            FightManager.LogicEntity.Init();
            FightManager.Damage = new DamageManager();
            FightManager.Damage.Init();
            uidSeed = 2000;
        }

        [TearDown]
        public void TearDown()
        {
            FightManager.LogicEntity?.Destroy();
            FightManager.Damage?.Destroy();
            GameManager.Dispatcher = oldDispatcher;
            FightManager.Config = oldConfig;
            FightManager.LogicEntity = oldLogicEntity;
            FightManager.Damage = oldDamageManager;
        }

        [Test]
        public void DamageQueue_ResolvesExtraDamageAfterCurrentBeHurt()
        {
            var trace = new List<string>();
            var attacker = CreateActorEntity();
            var defender = CreateActorEntity();

            attacker.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 1001,
                Data = CreateBuffData(1001, onHit: new TraceHitAction(trace, "attacker.onHit", DamageTag.Melee)),
                Creater = attacker,
                Target = attacker.Uid,
                AddStack = 1,
                Duration = 1f,
                IsOverrideDuration = true,
            });
            defender.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 1002,
                Data = CreateBuffData(1002, onBeHurt: new EnqueueExtraDamageOnBeHurtAction(trace, "defender.onBeHurt", attacker, defender)),
                Creater = attacker,
                Target = defender.Uid,
                AddStack = 1,
                Duration = 1f,
                IsOverrideDuration = true,
            });
            defender.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 1003,
                Data = CreateBuffData(1003, onBeHurt: new TraceBeHurtAction(trace, "queued.extraDamage", DamageTag.Extra)),
                Creater = attacker,
                Target = defender.Uid,
                AddStack = 1,
                Duration = 1f,
                IsOverrideDuration = true,
            });

            FightManager.Damage.Enqueue(new DamageInfo
            {
                Attacker = attacker,
                Defender = defender,
                Tags = DamageTag.Melee,
                TriggerHurtBehavior = false,
            });

            FightManager.Damage.Flush();

            CollectionAssert.AreEqual(
                new[] { "attacker.onHit", "defender.onBeHurt", "queued.extraDamage" },
                trace);
        }

        [Test]
        public void Resolve_LethalDamage_FiresKillHooksAndMarksDefenderDead()
        {
            var trace = new List<string>();
            var attacker = CreateActorEntity();
            var defender = CreateActorEntity();

            attacker.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 1004,
                Data = CreateBuffData(
                    1004,
                    onHit: new TraceHitAction(trace, "attacker.onHit", DamageTag.Melee),
                    onKill: new TraceKillAction(trace, "attacker.onKill")),
                Creater = attacker,
                Target = attacker.Uid,
                AddStack = 1,
                Duration = 1f,
                IsOverrideDuration = true,
            });
            defender.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 1005,
                Data = CreateBuffData(
                    1005,
                    onBeHurt: new TraceBeHurtAction(trace, "defender.onBeHurt", DamageTag.Melee),
                    onBeKilled: new TraceBeKilledAction(trace, "defender.onBeKilled")),
                Creater = attacker,
                Target = defender.Uid,
                AddStack = 1,
                Duration = 1f,
                IsOverrideDuration = true,
            });

            FightManager.Damage.Enqueue(new DamageInfo
            {
                Attacker = attacker,
                Defender = defender,
                Tags = DamageTag.Melee,
                TriggerHurtBehavior = false,
                IsLethal = true,
            });

            FightManager.Damage.Flush();

            Assert.That(defender.IsDead, Is.True);
            CollectionAssert.AreEqual(
                new[] { "attacker.onHit", "defender.onBeHurt", "attacker.onKill", "defender.onBeKilled" },
                trace);
        }

        [Test]
        public void Resolve_PendingBuffs_AppliesToDefenderAfterDamageCallbacks()
        {
            var attacker = CreateActorEntity();
            var defender = CreateActorEntity(attack: 100f);
            var rewardBuff = CreateBuffData(1006);
            rewardBuff.Modifier = new BuffModifierGroup
            {
                BuffPlus = new ActorAttr
                {
                    Attack = 10f,
                },
                ControlStateMod = ActorControlState.CreateDefault(),
            };

            FightManager.Damage.Enqueue(new DamageInfo
            {
                Attacker = attacker,
                Defender = defender,
                Tags = DamageTag.Melee,
                TriggerHurtBehavior = false,
                PendingBuffs = new List<AddBuffInfo>
                {
                    new AddBuffInfo
                    {
                        BuffId = rewardBuff.Id,
                        Data = rewardBuff,
                        Creater = attacker,
                        Target = defender.Uid,
                        AddStack = 1,
                        Duration = 1f,
                        IsOverrideDuration = true,
                    }
                },
            });

            FightManager.Damage.Flush();

            Assert.That(defender.GetComp<EffectComp>().GetBuff(rewardBuff.Id), Is.Not.Null);
            Assert.That(defender.GetComp<AttrComp>().FinalAttr.Attack, Is.EqualTo(110f));
        }

        Entity CreateActorEntity(float attack = 0f)
        {
            var actorData = ScriptableObject.CreateInstance<ActorData>();
            actorData.BaseAttr = new ActorAttr
            {
                Attack = attack,
            };
            actorData.BaseControlState = ActorControlState.CreateDefault();

            var entity = new Entity();
            entity.Init();
            entity.Bind(uidSeed++, EntityType.Actor);

            var dataComp = entity.AddComp<PlayerDataComp>();
            dataComp.Data = actorData;
            entity.AddComp<AttrComp>();
            entity.AddComp<EffectComp>();
            entity.AddComp<AttackComp>();
            return entity;
        }

        static BuffData CreateBuffData(
            int id,
            BuffHitAction onHit = null,
            BuffBeHurtAction onBeHurt = null,
            BuffKillAction onKill = null,
            BuffBeKilledAction onBeKilled = null)
        {
            var data = ScriptableObject.CreateInstance<BuffData>();
            data.Id = id;
            data.MaxStack = 5;
            data.Modifier = BuffModifierGroup.CreateDefault();
            data.OnHit = onHit;
            data.OnBeHurt = onBeHurt;
            data.OnKill = onKill;
            data.OnBeKilled = onBeKilled;
            return data;
        }

        class TraceHitAction : BuffHitAction
        {
            readonly List<string> trace;
            readonly string label;
            readonly DamageTag expectedTag;

            public TraceHitAction(List<string> trace, string label, DamageTag expectedTag)
            {
                this.trace = trace;
                this.label = label;
                this.expectedTag = expectedTag;
            }

            protected override void OnExecute()
            {
                if ((damage.Tags & expectedTag) != 0)
                {
                    trace.Add(label);
                }
            }
        }

        class TraceBeHurtAction : BuffBeHurtAction
        {
            readonly List<string> trace;
            readonly string label;
            readonly DamageTag expectedTag;

            public TraceBeHurtAction(List<string> trace, string label, DamageTag expectedTag)
            {
                this.trace = trace;
                this.label = label;
                this.expectedTag = expectedTag;
            }

            protected override void OnExecute()
            {
                if ((damage.Tags & expectedTag) != 0)
                {
                    trace.Add(label);
                }
            }
        }

        class TraceKillAction : BuffKillAction
        {
            readonly List<string> trace;
            readonly string label;

            public TraceKillAction(List<string> trace, string label)
            {
                this.trace = trace;
                this.label = label;
            }

            protected override void OnExecute()
            {
                trace.Add(label);
            }
        }

        class TraceBeKilledAction : BuffBeKilledAction
        {
            readonly List<string> trace;
            readonly string label;

            public TraceBeKilledAction(List<string> trace, string label)
            {
                this.trace = trace;
                this.label = label;
            }

            protected override void OnExecute()
            {
                trace.Add(label);
            }
        }

        class EnqueueExtraDamageOnBeHurtAction : BuffBeHurtAction
        {
            readonly List<string> trace;
            readonly string label;
            readonly Entity attacker;
            readonly Entity defender;

            public EnqueueExtraDamageOnBeHurtAction(List<string> trace, string label, Entity attacker, Entity defender)
            {
                this.trace = trace;
                this.label = label;
                this.attacker = attacker;
                this.defender = defender;
            }

            protected override void OnExecute()
            {
                if ((damage.Tags & DamageTag.Melee) == 0)
                {
                    return;
                }

                trace.Add(label);
                FightManager.Damage.Enqueue(new DamageInfo
                {
                    Attacker = attacker,
                    Defender = defender,
                    Tags = DamageTag.Extra,
                    TriggerHurtBehavior = false,
                });
            }
        }
    }
}
