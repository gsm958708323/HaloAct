namespace Combat
{
    public class ApplyProcessor : IEffectProcessor
    {
        private BulletFactory bulletFactory;
        private AoeFactory aoeFactory;

        public ApplyProcessor(BulletFactory bulletFactory, AoeFactory aoeFactory)
        {
            this.bulletFactory = bulletFactory;
            this.aoeFactory = aoeFactory;
        }

        public string Name => "执行效果";

        public void Process(EffectRequest request, World world)
        {
            throw new System.NotImplementedException();
        }
    }
}