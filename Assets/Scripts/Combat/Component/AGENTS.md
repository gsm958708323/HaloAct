┌──────────────────────────────────────────────────────────────────┐
│                    Component 原子库                               │
├──────────────────┬──────┬────┬────┬─────┬───────┬────┬─────────┤
│                  │ 角色 │ 子弹│ AOE│ Buff│ 召唤物│陷阱│ 环境效果│
├──────────────────┼──────┼────┼────┼─────┼───────┼────┼─────────┤
│ TransformComp    │  ●   │ ●  │ ●  │     │   ●   │ ●  │    ●   │
│ ColliderComp     │  ●   │ ●  │ ●  │     │   ●   │ ●  │        │
│ SourceInfoComp   │      │ ●  │ ●  │  ●  │   ●   │ ●  │    ●   │
│ LifetimeComp     │      │ ●  │ ●  │  ●  │   ○   │ ●  │    ○   │
│ EffectPayloadComp│      │ ●  │ ●  │  ●  │   ○   │ ●  │    ●   │
│ TickTimerComp    │      │    │ ●  │  ○  │       │ ○  │    ●   │
│ TargetMemoryComp │      │ ○  │ ●  │     │       │ ●  │        │
│ HitConfigComp    │      │ ●  │    │     │       │    │        │
│ TrajectoryComp   │      │ ●  │    │     │   ○   │    │        │
│ FollowComp       │      │ ○  │ ○  │     │   ●   │    │        │
│ FactionComp      │  ●   │ ●  │ ●  │     │   ●   │ ●  │        │
│ HealthComp       │  ●   │    │    │     │   ●   │ ○  │        │
│ BuffContainerComp│  ●   │    │    │     │   ●   │    │        │
│ ModifierCacheComp│  ●   │    │    │     │   ●   │    │        │
│ BuffCoreComp     │      │    │    │  ●  │       │    │        │
│ BuffStackComp    │      │    │    │  ○  │       │    │        │
│ BuffModifierComp │      │    │    │  ○  │       │    │        │
│ BuffTriggerComp  │      │    │    │  ○  │       │    │        │
│ BuffDependComp   │      │    │    │  ○  │       │    │        │
├──────────────────┴──────┴────┴────┴─────┴───────┴────┴─────────┤
│ ● = 必选   ○ = 可选（由配置决定）                                │
└──────────────────────────────────────────────────────────────────┘