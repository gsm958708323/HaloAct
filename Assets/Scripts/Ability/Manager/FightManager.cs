using HaloFrame;
using Ability;


public class FightManager : GameManagerBase
{
    public static ConfigManager Config;
    public static EntityManager LogicEntity;
    public static EntityRenderManager RenderEntity;
    public static BulletManager Bullet;
    public static AoeManager Aoe;
    public static DamageManager Damage;
    public static PlayerGameInput GameInput;
    
    protected override void InitManager()
    {
        base.InitManager();
        Config = GetManager<ConfigManager>();
        LogicEntity = GetManager<EntityManager>();
        RenderEntity = GetManager<EntityRenderManager>();
        Bullet = GetManager<BulletManager>();
        Aoe = GetManager<AoeManager>();
        Damage = GetManager<DamageManager>();

        GameInput = gameObject.AddComponent<PlayerGameInput>();
    }
}
