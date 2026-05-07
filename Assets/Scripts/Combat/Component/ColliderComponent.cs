namespace Combat
{
    public class ColliderComponent : IComponent
    {
        public ColliderData Data;

        /// <summary>
        /// 是否用作扇形检测的包围球。
        /// true 时需要配合 SourceInfoComponent.IsSector 做角度过滤。
        /// </summary>
        public bool IsSectorBounding;
        public float SectorHalfAngle;
    }
}