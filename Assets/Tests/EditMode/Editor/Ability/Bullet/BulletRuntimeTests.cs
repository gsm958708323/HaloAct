using System.Collections.Generic;
using Ability;
using HaloFrame;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ability.Bullet
{
    public class BulletRuntimeTests
    {
        Dispatcher oldDispatcher;
        ConfigManager oldConfig;
        EntityManager oldLogicEntity;
        BulletManager oldBulletManager;
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
            oldDamageManager = FightManager.Damage;

            GameManager.Dispatcher = new Dispatcher();
            FightManager.Config = new ConfigManager();

            testLogicEntity = new TestEntityManager();
            testLogicEntity.Init();
            FightManager.LogicEntity = testLogicEntity;

            FightManager.Bullet = new BulletManager();
            FightManager.Bullet.Init();
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
            FightManager.Damage?.Destroy();

            GameManager.Dispatcher = oldDispatcher;
            FightManager.Config = oldConfig;
            FightManager.LogicEntity = oldLogicEntity;
            FightManager.Bullet = oldBulletManager;
            FightManager.Damage = oldDamageManager;
        }

        [Test]
        public void Bullet_RemovalReasonDistinguishesLifetimeAndObstacle()
        {
            var removeReasons = new List<BulletRemoveReason>();

            CreateBullet(
                CreateBulletData(
                    3001,
                    duration: 0.05f,
                    speed: 0f,
                    onRemoved: new TraceBulletRemovedAction(removeReasons)),
                null);

            FightManager.Bullet.Tick(0.1f);

            Assert.That(removeReasons, Has.Count.EqualTo(1));
            Assert.That(removeReasons[0], Is.EqualTo(BulletRemoveReason.LifetimeEnded));

            removeReasons.Clear();
            CreateObstacle(Vector3.forward * 0.5f, new Vector3(1f, 1f, 1f));

            CreateBullet(
                CreateBulletData(
                    3002,
                    duration: 1f,
                    speed: 1f,
                    radius: 0.1f,
                    onRemoved: new TraceBulletRemovedAction(removeReasons)),
                null);

            Physics.SyncTransforms();
            FightManager.Bullet.Tick(1f);

            Assert.That(removeReasons, Has.Count.EqualTo(1));
            Assert.That(removeReasons[0], Is.EqualTo(BulletRemoveReason.ObstacleHit));
        }

        [Test]
        public void Bullet_HitTarget_QueuesDamageAndUsesHitLimitRemoval()
        {
            var trace = new List<string>();
            var removeReasons = new List<BulletRemoveReason>();
            var attacker = CreateActorEntity(ActorType.PLAYER);
            var defender = CreateActorEntity(ActorType.Enemy, 100f);
            CreateTargetCollider(defender, new Vector3(0f, 0f, 0.6f), 0.2f);

            defender.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 1001,
                Data = CreateBuffData(1001, new TraceBeHurtAction(trace, "defender.onBeHurt", DamageTag.Bullet)),
                Creater = attacker,
                Target = defender.Uid,
                AddStack = 1,
                Duration = 1f,
                IsOverrideDuration = true,
            });

            CreateBullet(
                CreateBulletData(
                    3003,
                    duration: 1f,
                    speed: 1f,
                    radius: 0.1f,
                    hitTimes: 1,
                    onRemoved: new TraceBulletRemovedAction(removeReasons)),
                attacker);

            Physics.SyncTransforms();
            FightManager.Bullet.Tick(1f);
            FightManager.Damage.Flush();

            CollectionAssert.AreEqual(new[] { "defender.onBeHurt" }, trace);
            Assert.That(removeReasons, Has.Count.EqualTo(1));
            Assert.That(removeReasons[0], Is.EqualTo(BulletRemoveReason.HitLimitReached));
        }

        [Test]
        public void Bullet_CanHitAfterCreated_DelaysFirstValidHit()
        {
            var trace = new List<string>();
            var attacker = CreateActorEntity(ActorType.PLAYER);
            var defender = CreateActorEntity(ActorType.Enemy);
            CreateTargetCollider(defender, new Vector3(0f, 0f, 0.25f), 0.2f);

            defender.GetComp<EffectComp>().AddBuff(new AddBuffInfo
            {
                BuffId = 1002,
                Data = CreateBuffData(1002, new TraceBeHurtAction(trace, "defender.onBeHurt", DamageTag.Bullet)),
                Creater = attacker,
                Target = defender.Uid,
                AddStack = 1,
                Duration = 1f,
                IsOverrideDuration = true,
            });

            var bullet = CreateBullet(
                CreateBulletData(
                    3004,
                    duration: 1f,
                    speed: 1f,
                    radius: 0.1f,
                    hitTimes: 1,
                    canHitAfterCreated: 0.3f),
                attacker);

            Physics.SyncTransforms();
            FightManager.Bullet.Tick(0.1f);
            FightManager.Damage.Flush();

            Assert.That(trace, Is.Empty);
            Assert.That(FightManager.LogicEntity.GetEntity(bullet.Uid), Is.Not.Null);

            FightManager.Bullet.Tick(0.25f);
            FightManager.Damage.Flush();

            CollectionAssert.AreEqual(new[] { "defender.onBeHurt" }, trace);
        }

        Entity CreateActorEntity(ActorType actorType, float attack = 0f)
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

        Entity CreateBullet(BulletData data, Entity caster)
        {
            return FightManager.LogicEntity.CreateBullet(new BulletLauncher
            {
                BulletId = data.Id,
                Data = data,
                Caster = caster,
                Position = Vector3.zero,
                Direction = Vector3.forward,
            });
        }

        BulletData CreateBulletData(
            int id,
            float duration,
            float speed,
            float radius = 0.1f,
            int hitTimes = 1,
            float canHitAfterCreated = 0f,
            BulletAction onRemoved = null)
        {
            var data = ScriptableObject.CreateInstance<BulletData>();
            data.Id = id;
            data.Duration = duration;
            data.Speed = speed;
            data.Radius = radius;
            data.HitTimes = hitTimes;
            data.HitFoe = true;
            data.HitAlly = false;
            data.RemoveOnObstacle = true;
            data.CanHitAfterCreated = canHitAfterCreated;
            data.OnRemoved = onRemoved;
            createdObjects.Add(data);
            return data;
        }

        BuffData CreateBuffData(int id, BuffBeHurtAction onBeHurt)
        {
            var data = ScriptableObject.CreateInstance<BuffData>();
            data.Id = id;
            data.MaxStack = 1;
            data.Modifier = BuffModifierGroup.CreateDefault();
            data.OnBeHurt = onBeHurt;
            createdObjects.Add(data);
            return data;
        }

        void CreateTargetCollider(Entity entity, Vector3 position, float radius)
        {
            var go = new GameObject($"Target-{entity.Uid}");
            go.transform.position = position;
            var collider = go.AddComponent<SphereCollider>();
            collider.radius = radius;
            go.AddComponent<HurtBox>();
            var idCard = go.AddComponent<IdentitCard>();
            idCard.Uid = entity.Uid;
            createdObjects.Add(go);
        }

        void CreateObstacle(Vector3 position, Vector3 scale)
        {
            var go = new GameObject("Obstacle");
            go.transform.position = position;
            go.transform.localScale = scale;
            go.AddComponent<BoxCollider>();
            createdObjects.Add(go);
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

        class TraceBulletRemovedAction : BulletAction
        {
            readonly List<BulletRemoveReason> removeReasons;

            public TraceBulletRemovedAction(List<BulletRemoveReason> removeReasons)
            {
                this.removeReasons = removeReasons;
            }

            protected override void OnExecute()
            {
                removeReasons.Add(bullet.RemoveReason);
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
                if (damage.HasTag(expectedTag))
                {
                    trace.Add(label);
                }
            }
        }
    }
}
