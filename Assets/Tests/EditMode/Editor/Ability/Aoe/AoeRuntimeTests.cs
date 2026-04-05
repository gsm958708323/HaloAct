using System.Collections.Generic;
using Ability;
using HaloFrame;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ability.Aoe
{
    public class AoeRuntimeTests
    {
        Dispatcher oldDispatcher;
        ConfigManager oldConfig;
        EntityManager oldLogicEntity;
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
            oldAoeManager = FightManager.Aoe;
            oldDamageManager = FightManager.Damage;

            GameManager.Dispatcher = new Dispatcher();
            FightManager.Config = new ConfigManager();

            testLogicEntity = new TestEntityManager();
            testLogicEntity.Init();
            FightManager.LogicEntity = testLogicEntity;

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
            FightManager.Aoe?.Destroy();
            FightManager.Damage?.Destroy();

            GameManager.Dispatcher = oldDispatcher;
            FightManager.Config = oldConfig;
            FightManager.LogicEntity = oldLogicEntity;
            FightManager.Aoe = oldAoeManager;
            FightManager.Damage = oldDamageManager;
        }

        [Test]
        public void Aoe_TracksEnterTickLeaveAndRemoved()
        {
            var trace = new List<string>();
            var caster = CreateActorEntity(ActorType.PLAYER);
            var target = CreateActorEntity(ActorType.Enemy);
            var targetGo = CreateTargetCollider(target, new Vector3(5f, 0f, 0f), 0.25f);
            var aoeData = CreateAoeData(
                4001,
                radius: 1f,
                duration: 5f,
                onCreate: new TraceAoeAction(trace, "create"),
                onEnter: new TraceAoeAction(trace, "enter"),
                onTick: new TraceAoeAction(trace, "tick"),
                onLeave: new TraceAoeAction(trace, "leave"),
                onRemoved: new TraceAoeAction(trace, "removed"));

            var aoe = FightManager.LogicEntity.CreateAoe(new AoeLauncher
            {
                AoeId = aoeData.Id,
                Data = aoeData,
                Caster = caster,
                Position = Vector3.zero,
            });

            targetGo.transform.position = new Vector3(0.5f, 0f, 0f);
            Physics.SyncTransforms();
            FightManager.Aoe.Tick(0.1f);

            targetGo.transform.position = new Vector3(5f, 0f, 0f);
            Physics.SyncTransforms();
            FightManager.Aoe.Tick(0.1f);

            FightManager.LogicEntity.RemoveEntity(aoe.Uid);

            CollectionAssert.AreEqual(
                new[] { "create", "enter", "tick", "leave", "removed" },
                trace);
        }

        [Test]
        public void Aoe_TracksBulletTargetsSeparately()
        {
            var trace = new List<string>();
            var caster = CreateActorEntity(ActorType.PLAYER);
            var bullet = CreateBulletEntity();
            var bulletGo = CreateTargetCollider(bullet, new Vector3(5f, 0f, 0f), 0.2f);
            var aoeData = CreateAoeData(
                4002,
                radius: 1f,
                duration: 5f,
                affectBullets: true,
                onEnter: new TraceAoeAction(trace, "bullet-enter"),
                onTick: new TraceAoeAction(trace, "bullet-tick"),
                onLeave: new TraceAoeAction(trace, "bullet-leave"));

            FightManager.LogicEntity.CreateAoe(new AoeLauncher
            {
                AoeId = aoeData.Id,
                Data = aoeData,
                Caster = caster,
                Position = Vector3.zero,
            });

            bulletGo.transform.position = new Vector3(0.4f, 0f, 0f);
            Physics.SyncTransforms();
            FightManager.Aoe.Tick(0.1f);

            bulletGo.transform.position = new Vector3(5f, 0f, 0f);
            Physics.SyncTransforms();
            FightManager.Aoe.Tick(0.1f);

            CollectionAssert.AreEqual(
                new[] { "bullet-enter", "bullet-tick", "bullet-leave" },
                trace);
        }

        [Test]
        public void Aoe_DurationEnd_RemovedFiresOnce()
        {
            var trace = new List<string>();
            var caster = CreateActorEntity(ActorType.PLAYER);
            var aoeData = CreateAoeData(
                4003,
                radius: 1f,
                duration: 0.1f,
                onRemoved: new TraceAoeAction(trace, "removed"));

            FightManager.LogicEntity.CreateAoe(new AoeLauncher
            {
                AoeId = aoeData.Id,
                Data = aoeData,
                Caster = caster,
                Position = Vector3.zero,
            });

            FightManager.Aoe.Tick(0.2f);
            FightManager.Aoe.Tick(0.2f);

            CollectionAssert.AreEqual(new[] { "removed" }, trace);
        }

        Entity CreateActorEntity(ActorType actorType)
        {
            var actorData = ScriptableObject.CreateInstance<ActorData>();
            actorData.ActorType = actorType;
            actorData.BaseControlState = ActorControlState.CreateDefault();
            createdObjects.Add(actorData);
            return testLogicEntity.CreateTestActor(actorData);
        }

        Entity CreateBulletEntity()
        {
            var bulletData = ScriptableObject.CreateInstance<BulletData>();
            bulletData.Id = 9001;
            createdObjects.Add(bulletData);
            return testLogicEntity.CreateTestBullet(bulletData);
        }

        GameObject CreateTargetCollider(Entity entity, Vector3 position, float radius)
        {
            var go = new GameObject($"Target-{entity.Uid}");
            go.transform.position = position;
            var collider = go.AddComponent<SphereCollider>();
            collider.radius = radius;
            var idCard = go.AddComponent<IdentitCard>();
            idCard.Uid = entity.Uid;
            createdObjects.Add(go);
            return go;
        }

        AoeData CreateAoeData(
            int id,
            float radius,
            float duration,
            bool affectBullets = false,
            AoeAction onCreate = null,
            AoeAction onEnter = null,
            AoeAction onTick = null,
            AoeAction onLeave = null,
            AoeAction onRemoved = null)
        {
            var data = ScriptableObject.CreateInstance<AoeData>();
            data.Id = id;
            data.Radius = radius;
            data.Duration = duration;
            data.TickInterval = 0f;
            data.HitFoe = true;
            data.HitAlly = false;
            data.AffectActors = true;
            data.AffectBullets = affectBullets;
            data.OnCreate = onCreate;
            data.OnEnter = onEnter;
            data.OnTick = onTick;
            data.OnLeave = onLeave;
            data.OnRemoved = onRemoved;
            createdObjects.Add(data);
            return data;
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

            public Entity CreateTestBullet(BulletData data)
            {
                var entity = AddEntity(EntityType.Bullet);
                var dataComp = entity.AddComp<BulletDataComp>();
                dataComp.Data = data;
                return entity;
            }
        }

        class TraceAoeAction : AoeAction
        {
            readonly List<string> trace;
            readonly string label;

            public TraceAoeAction(List<string> trace, string label)
            {
                this.trace = trace;
                this.label = label;
            }

            protected override void OnExecute()
            {
                trace.Add(label);
            }
        }
    }
}
