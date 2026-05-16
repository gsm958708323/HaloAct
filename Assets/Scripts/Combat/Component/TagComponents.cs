namespace Combat
{
    /// <summary>
    /// 标记 Entity 待销毁。帧末由 EntityCleanupSystem 处理。
    /// </summary>
    public class DestroyTagComponent : IComponent
    {
    }

    /// <summary>
    /// 标记本帧 Tick 到期。帧末由 TickReadyCleanupSystem 清除。
    /// </summary>
    public class TickReadyTagComponent : IComponent
    {
    }
}