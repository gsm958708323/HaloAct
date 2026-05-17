using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat;

public class CombatBootstrap : MonoBehaviour
{
    public World World { get; private set; }
    public EffectRequestBuffer EffectBuffer { get; private set; }
    public IEventBus EventBus { get; private set; }
    public ConfigManager ConfigManager { get; private set; }
    public BuffFactory BuffFactory { get; private set; }
    public BulletFactory BulletFactory { get; private set; }
    public AoeFactory AOEFactory { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        ConfigManager = new ConfigManager();
        ConfigManager.LoadAll();

        EventBus = new EventBus();
        EffectBuffer = new EffectRequestBuffer();
        World = new World();

        BuffFactory = new BuffFactory(
                World, ConfigManager, EffectBuffer, EventBus);
        BulletFactory = new BulletFactory(World, ConfigManager);
        AOEFactory = new AoeFactory(World, ConfigManager);

        // --- 注册 System ---
        World.RegisterSystem(
            new FollowSystem(), 100);
        World.RegisterSystem(
            new BuffLifecycleSystem(EffectBuffer, EventBus), 110);
        World.RegisterSystem(
            new TickSystem(), 120);
        World.RegisterSystem(
            new BuffTickEffectSystem(EffectBuffer), 130);
        World.RegisterSystem(
            new BuffModifierRebuildSystem(), 140);
        World.RegisterSystem(
            new LifetimeSystem(EffectBuffer), 150);
        World.RegisterSystem(
            new BulletMoveSystem(), 200);
        World.RegisterSystem(
            new AOETargetDetectionSystem(EffectBuffer), 300);
        World.RegisterSystem(
            new CollisionSystem(EffectBuffer), 400);
        World.RegisterSystem(
            new EffectPipelineSystem(
                EffectBuffer, EventBus,
                BuffFactory, BulletFactory, AOEFactory), 500);
        World.RegisterSystem(
            new AOECleanupSystem(EffectBuffer), 860);
        World.RegisterSystem(
            new EntityCleanupSystem(), 900);
    }

    // Update is called once per frame
    void Update()
    {
        World.Tick(Time.deltaTime);
    }
}
