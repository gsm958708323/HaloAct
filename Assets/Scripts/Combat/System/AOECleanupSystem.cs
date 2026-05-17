namespace Combat
{
    /// <summary>
    /// AOE 销毁前，对仍在区域内的目标触发 OnExit 效果。
    /// </summary>
    public class AOECleanupSystem : ISystem
    {
        private readonly EffectRequestBuffer effectBuffer;
        public AOECleanupSystem(EffectRequestBuffer effectBuffer)
        {
            this.effectBuffer = effectBuffer;
        }

        public override void Tick(float delteTime)
        {
            var entities = world.Query<DestroyTagComponent, TargetMemoryComponent>();

            foreach (var entity in entities)
            {
                var sourceInfo = world.GetComponent<SourceInfoComponent>(entity);
                var payload = world.GetComponent<EffectPayloadComponent>(entity);
                var memory = world.GetComponent<TargetMemoryComponent>(entity);

                if (sourceInfo == null || payload == null || memory == null)
                    continue;

                var exitEffects = PayloadHelper.GetEffects(
                    payload, PayloadTrigger.OnExit);
                if (exitEffects.Length == 0) continue;

                // Previous 中的所有目标视为"离开"
                foreach (var target in memory.Previous)
                {
                    if (!world.IsAlive(target)) continue;

                    EffectSubmitHelper.Submit(effectBuffer,
                        sourceInfo.Source, entity, target,
                        exitEffects);
                }
            }
        }
    }
}