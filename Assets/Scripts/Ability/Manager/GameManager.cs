using Frame;

namespace Ability
{
    public class GameManager : GameManagerBase
    {
        public static DriverManager DriverManager;
        public static ConfigManager Config;
        public static ActorManager Actor;
        public static BulletManager Bullet;

        protected override void InitManager()
        {
            base.InitManager();

            Config = GetManager<ConfigManager>();
            DriverManager = GetManager<DriverManager>();
            Actor = GetManager<ActorManager>();
            Bullet = GetManager<BulletManager>();
        }
    }
}