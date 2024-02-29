using Frame;

namespace Ability
{
    public class GameManager : GameManagerBase
    {
        public static DriverManager DriverManager;
        public static ActorManager ActorManager;

        protected override void InitManager()
        {
            base.InitManager();

            DriverManager = GetManager<DriverManager>();
            ActorManager = GetManager<ActorManager>();
        }
    }
}