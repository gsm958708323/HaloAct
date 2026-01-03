using HaloFrame;


public class GameManager : GameManagerBase
{
    public static DriverManager DriverManager;
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
        // Resource = GetManager<ResourceManager>();
        // UI = GetManager<UIManager>();
    }
}
