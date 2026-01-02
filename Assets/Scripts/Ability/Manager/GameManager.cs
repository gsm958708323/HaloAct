using HaloFrame;
using Ability;


public class GameManager : GameManagerBase
{
    public static DriverManager DriverManager;
    public static ConfigManager Config;
    public static EntityManager LogicEntity;
    public static EntityRenderManager RenderEntity;
    public static BulletManager Bullet;
    public static PlayerGameInput GameInput;
    public static Dispatcher Dispatcher;
    public static RedDotManager RedDot;
    public static UIManager UI;
    public static ResourceManager Resource;
    public static DownloadManager Download;

    protected override void InitManager()
    {
        base.InitManager();

        Dispatcher = GetManager<Dispatcher>();
        DriverManager = GetManager<DriverManager>();
        RedDot = GetManager<RedDotManager>();
        Download = GetManager<DownloadManager>();
        Resource = GetManager<ResourceManager>();
        UI = GetManager<UIManager>();

        Config = GetManager<ConfigManager>();
        LogicEntity = GetManager<EntityManager>();
        RenderEntity = GetManager<EntityRenderManager>();
        Bullet = GetManager<BulletManager>();

        GameInput = gameObject.AddComponent<PlayerGameInput>();
    }
}
