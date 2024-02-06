using Frame;

namespace Ability
{
    public class GameManager : GameManagerBase
    {
        public static BulletManager BulletManager;
        public static DriverManager DriverManager;
        public static ActorManager ActorManager;

        protected override void InitManager()
        {
            base.InitManager();

            BulletManager = GetManager<BulletManager>();
            DriverManager = GetManager<DriverManager>();
            ActorManager = GetManager<ActorManager>();
        }
    }
}