using System.Collections.Generic;

namespace Combat
{
    public static class TrajectoryStrategies
    {
        private static readonly Dictionary<TrajectoryType, ITrajectoryStrategy> Map = new()
    {
        { TrajectoryType.Straight,  new StraightTrajectory() },
        { TrajectoryType.Parabola,  new ParabolaTrajectory() },
    };

        public static ITrajectoryStrategy Get(TrajectoryType type) => Map[type];
    }
}