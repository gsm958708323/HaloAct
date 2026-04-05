using NUnit.Framework;
using UnityEngine;
using HaloFrame;

namespace Ability.Combat
{
    public class AttrCompTests
    {
        Dispatcher oldDispatcher;
        ConfigManager oldConfig;
        EntityManager oldLogicEntity;

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
        public void AttrComp_LoadsLegacyActorDataIntoFinalState()
        {
            var actorData = ScriptableObject.CreateInstance<ActorData>();
            actorData.Gravity = -20f;
            actorData.DelayAerialTime = 0.5f;
            actorData.BaseControlState = new ActorControlState
            {
                CanMove = false,
                CanRotate = true,
                CanUseSkill = false,
                CanAttack = true,
                CanBeControlled = false,
            };

            var entity = new Entity();
            entity.Init();
            var dataComp = entity.AddComp<PlayerDataComp>();
            dataComp.Data = actorData;

            var attrComp = entity.AddComp<AttrComp>();

            Assert.That(attrComp.FinalAttr.Gravity, Is.EqualTo(-20f));
            Assert.That(attrComp.FinalAttr.DelayAerialTime, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(attrComp.FinalControlState.CanMove, Is.False);
            Assert.That(attrComp.FinalControlState.CanRotate, Is.True);
            Assert.That(attrComp.FinalControlState.CanUseSkill, Is.False);
        }

        [Test]
        public void CreateActor_LoadsLegacyResourceActorDataIntoAttrComp()
        {
            var entity = FightManager.LogicEntity.CreateActor(1001);
            var attrComp = entity?.GetComp<AttrComp>();

            Assert.That(entity, Is.Not.Null);
            Assert.That(attrComp, Is.Not.Null);
            Assert.That(attrComp.FinalAttr.Gravity, Is.EqualTo(-20f));
            Assert.That(attrComp.FinalAttr.DelayAerialTime, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void Recheck_AppliesPlusThenRatioModifiers()
        {
            var entity = new Entity();
            entity.Init();
            entity.AddComp<PlayerDataComp>();
            var attrComp = entity.AddComp<AttrComp>();

            attrComp.SetBaseAttr(new ActorAttr
            {
                Attack = 100,
                MoveSpeed = 10f,
            });
            attrComp.AddPlusAttr(new ActorAttr
            {
                Attack = 20,
                MoveSpeed = 2f,
            });
            attrComp.AddRatioAttr(new ActorAttr
            {
                Attack = 0.5f,
                MoveSpeed = 0.1f,
            });

            Assert.That(attrComp.FinalAttr.Attack, Is.EqualTo(180));
            Assert.That(attrComp.FinalAttr.MoveSpeed, Is.EqualTo(13.2f).Within(0.001f));
        }

        [Test]
        public void ResetRuntimeState_ClearsRuntimeModifiersAndRestoresBaseControlState()
        {
            var entity = new Entity();
            entity.Init();
            entity.AddComp<PlayerDataComp>();
            var attrComp = entity.AddComp<AttrComp>();

            var baseControlState = new ActorControlState
            {
                CanMove = true,
                CanRotate = false,
                CanUseSkill = true,
                CanAttack = true,
                CanBeControlled = false,
            };

            attrComp.SetBaseAttr(new ActorAttr
            {
                Attack = 80,
                MoveSpeed = 8f,
            });
            attrComp.SetBaseControlState(baseControlState);
            attrComp.AddPlusAttr(new ActorAttr
            {
                Attack = 20,
                MoveSpeed = 2f,
            });
            attrComp.AddRatioAttr(new ActorAttr
            {
                Attack = 0.5f,
                MoveSpeed = 0.25f,
            });
            attrComp.SetRuntimeControlState(new ActorControlState
            {
                CanMove = false,
                CanRotate = true,
                CanUseSkill = false,
                CanAttack = true,
                CanBeControlled = true,
            });

            attrComp.ResetRuntimeState();

            Assert.That(attrComp.FinalAttr.Attack, Is.EqualTo(80));
            Assert.That(attrComp.FinalAttr.MoveSpeed, Is.EqualTo(8f).Within(0.001f));
            Assert.That(attrComp.FinalControlState.CanMove, Is.True);
            Assert.That(attrComp.FinalControlState.CanRotate, Is.False);
            Assert.That(attrComp.FinalControlState.CanUseSkill, Is.True);
            Assert.That(attrComp.FinalControlState.CanAttack, Is.True);
            Assert.That(attrComp.FinalControlState.CanBeControlled, Is.False);
        }
    }
}
