namespace Combat
{
    public class TickSystem : ISystem
    {
        public override void Tick(float delteTime)
        {
            foreach (var entity in world.Query<TickTimerComponent>())
            {
                var tick = world.GetComponent<TickTimerComponent>(entity);
                tick.Timer -= delteTime;

                while(tick.Timer <= 0)
                {
                    tick.Timer += tick.Interval;
                    tick.TickCount ++;

                    // TickSys只负责即时，做哪些其他Sys消费Tag即可
                    if(!world.HasComponent<TickReadyTagComponent>(entity))
                    {
                        world.AddComponent<TickReadyTagComponent>(entity);
                    }
                    if(tick.MaxTicks>0 && tick.TickCount>= tick.MaxTicks)
                    {
                        world.AddComponent<DestroyTagComponent>(entity);
                        break;
                    }

                    // 单帧最多补偿5次
                    if(tick.TickCount % 5 == 0) break;
                }
            }
        }
    }
}