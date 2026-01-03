using HaloFrame;
using Ability;


public class FightManager : GameManagerBase
{
    public static ConfigManager Config;
    public static EntityManager LogicEntity;
    public static EntityRenderManager RenderEntity;
    public static BulletManager Bullet;
    public static PlayerGameInput GameInput;
    
    protected override void InitManager()
    {
        base.InitManager();
        Config = GetManager<ConfigManager>();
        LogicEntity = GetManager<EntityManager>();
        RenderEntity = GetManager<EntityRenderManager>();
        Bullet = GetManager<BulletManager>();

        GameInput = gameObject.AddComponent<PlayerGameInput>();
    }
}
