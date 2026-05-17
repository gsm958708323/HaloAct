using System.Collections.Generic;

namespace Combat
{
    public class BuffContainerComponent : IComponent
    {
        public List<BuffInstance> Instances { get; internal set; }
    }
}