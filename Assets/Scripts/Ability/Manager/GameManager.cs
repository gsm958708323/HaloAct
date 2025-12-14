using Frame;

namespace Ability
{
    public class GameManager : GameManagerBase
    {
        public static DriverManager DriverManager;
        public static ActorManager Actor;
        public static ConfigManager Config;

        protected override void InitManager()
        {
            base.InitManager();

            Config = GetManager<ConfigManager>();
            DriverManager = GetManager<DriverManager>();
            Actor = GetManager<ActorManager>();
        }
    }
}