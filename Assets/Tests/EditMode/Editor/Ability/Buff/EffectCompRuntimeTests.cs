using System.Collections.Generic;
using HaloFrame;
using NUnit.Framework;
using UnityEngine;

namespace Ability.Buff
{
    public class EffectCompRuntimeTests
    {
        Dispatcher oldDispatcher;
        ConfigManager oldConfig;
        EntityManager oldLogicEntity;
        int uidSeed;

        [SetUp]
        public void SetUp()
        {
            oldDispatcher = GameManager.Dispatcher;
            oldConfig = FightManager.Config;
            oldLogicEntity = FightManager.LogicEntity;

            GameManager.Dispatcher = new Dispatcher();
            FightManager.Config = new ConfigManager();
            FightManager.LogicEntity = new EntityManager();
            FightManager.LogicEntity.Init();
            uidSeed = 1000;
        }

        [TearDown]
        public void TearDown()
        {
            FightManager.LogicEntity?.Destroy();
            GameManager.Dispatcher = oldDispatcher;
            FightManager.Config = oldConfig;
            FightManager.LogicEntity = oldLogicEntity;
        }

        [Test]
        public void EnqueueBuff_StacksRefreshesAndRechecksAttrOnFlush()
        {
            var target = CreateActorEntity(attack: 100f);
            var caster = CreateActorEntity();
            var effects = target.GetComp<EffectComp>();
            var buffData = CreateBuffData(1001);

            buffData.Modifier = new BuffModifierGroup
            {
                BuffPlus = new ActorAttr
                {
                    Attack = 10f,
                },
                ControlStateMod = ActorControlState.CreateDefault(),
            };

            effects.EnqueueBuff(new AddBuffInfo
            {
                BuffId = buffData.Id,
                Data = buffData,
                Creater = caster,
                Target = target.Uid,
                AddStack = 1,
                Duration = 2f,
                IsOverrideDuration = true,
            });

            Assert.That(effects.GetBuff(buffData.Id), Is.Null);

            effects.FlushPending();
            effects.EnqueueBuff(new AddBuffInfo
            {
                BuffId = buffData.Id,
                Data = buffData,
                Creater = caster,
                Target = target.Uid,
                AddStack = 2,
                Duration = 1f,
                IsOverrideDuration = false,
            });
            effects.FlushPending();

            var buff = effects.GetBuff(buffData.Id);
            Assert.That(buff, Is.Not.Null);
            Assert.That(buff.Stack, Is.EqualTo(3));
            Assert.That(buff.Duration, Is.EqualTo(3f).Within(0.001f));
            Assert.That(target.GetComp<AttrComp>().FinalAttr.Attack, Is.EqualTo(130f));
        }

        [Test]
        public void Tick_ExpiredBuff_RemovesOnceAndRestoresAttr()
        {
            var trace = new List<string>();
            var target = CreateActorEntity(attack: 100f);
            var caster = CreateActorEntity();
            var effects = target.GetComp<EffectComp>();
            var buffData = CreateBuffData(1001);

            buffData.Modifier = new BuffModifierGroup
            {
                BuffPlus = new ActorAttr
                {
                    Attack = 20f,
                },
                ControlStateMod = ActorControlState.CreateDefault(),
            };
            buffData.OnRemoved = new TraceRemovedAction(trace, "removed");

            effects.EnqueueBuff(new AddBuffInfo
            {
                BuffId = buffData.Id,
                Data = buffData,
                Creater = caster,
                Target = target.Uid,
                AddStack = 1,
                Duration = 0.1f,
                IsOverrideDuration = true,
            });
            effects.FlushPending();

            Assert.That(target.GetComp<AttrComp>().FinalAttr.Attack, Is.EqualTo(120f));

            effects.Tick(0.2f);
            effects.Tick(0.2f);

            CollectionAssert.AreEqual(new[] { "removed" }, trace);
            Assert.That(effects.GetBuff(buffData.Id), Is.Null);
            Assert.That(target.GetComp<AttrComp>().FinalAttr.Attack, Is.EqualTo(100f));
        }

        [Test]
        public void Tick_OnRemovedQueuedBuffAppliesFollowUpAfterCurrentLoop()
        {
            var trace = new List<string>();
            var target = CreateActorEntity(attack: 100f);
            var caster = CreateActorEntity();
            var effects = target.GetComp<EffectComp>();
            var nextBuff = CreateBuffData(1002);
            var expiringBuff = CreateBuffData(1001);

            nextBuff.OnOccur = new TraceOccurAction(trace, "occur-1002");
            expiringBuff.OnRemoved = new EnqueueBuffOnRemoveAction(trace, nextBuff, caster, target);

            effects.EnqueueBuff(new AddBuffInfo
            {
                BuffId = expiringBuff.Id,
                Data = expiringBuff,
                Creater = caster,
                Target = target.Uid,
                AddStack = 1,
                Duration = 0.1f,
                IsOverrideDuration = true,
            });
            effects.FlushPending();
            trace.Clear();

            effects.Tick(0.2f);

            CollectionAssert.AreEqual(new[] { "removed-1001", "occur-1002" }, trace);
            Assert.That(effects.GetBuff(expiringBuff.Id), Is.Null);
            Assert.That(effects.GetBuff(nextBuff.Id), Is.Not.Null);
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
            return entity;
        }

        static BuffData CreateBuffData(int id)
        {
            var data = ScriptableObject.CreateInstance<BuffData>();
            data.Id = id;
            data.MaxStack = 5;
            data.Modifier = BuffModifierGroup.CreateDefault();
            return data;
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

        class TraceRemovedAction : BuffRemovedAction
        {
            readonly List<string> trace;
            readonly string label;

            public TraceRemovedAction(List<string> trace, string label)
            {
                this.trace = trace;
                this.label = label;
            }

            protected override void OnExecute()
            {
                trace.Add(label);
            }
        }

        class EnqueueBuffOnRemoveAction : BuffRemovedAction
        {
            readonly List<string> trace;
            readonly BuffData nextBuff;
            readonly Entity caster;
            readonly Entity target;

            public EnqueueBuffOnRemoveAction(List<string> trace, BuffData nextBuff, Entity caster, Entity target)
            {
                this.trace = trace;
                this.nextBuff = nextBuff;
                this.caster = caster;
                this.target = target;
            }

            protected override void OnExecute()
            {
                trace.Add("removed-1001");
                buff.Target.GetComp<EffectComp>().EnqueueBuff(new AddBuffInfo
                {
                    BuffId = nextBuff.Id,
                    Data = nextBuff,
                    Creater = caster,
                    Target = target.Uid,
                    AddStack = 1,
                    Duration = 1f,
                    IsOverrideDuration = true,
                });
            }
        }
    }
}
