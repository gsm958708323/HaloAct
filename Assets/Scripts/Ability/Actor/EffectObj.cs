using UnityEngine;

namespace Ability
{
    /// <summary>
    /// buff数据处理，存储动态数据和配置
    /// </summary>
    public class EffectObj : ILogicT<BuffData>
    {
        public BuffData BuffData;
        public Entity Creater { get; private set; }
        public Entity Target { get; private set; }

        public bool Permanent { get; private set; }
        public int Stack { get; private set; }
        public float TimeElapsed { get; private set; }
        public float Duration { get; private set; }
        public int Ticked { get; private set; }
        public object Param;

        float tickTime;

        public void Enter(BuffData t)
        {
            BuffData = t;
        }

        public void Exit()
        {
            BuffData = null;
            Creater = null;
            Target = null;
            Param = null;
        }

        public void Init()
        {
        }

        public void Tick(float deltaTime)
        {
        }

        public void Apply(Entity targetEntity, AddBuffInfo addInfo)
        {
            Creater = addInfo.Creater;
            Target = targetEntity;
            Permanent = addInfo.Permanent;

            if (addInfo.IsOverrideDuration || addInfo.Duration != 0f || Duration == 0f)
            {
                ModDuration(addInfo.Duration, addInfo.IsOverrideDuration);
            }

            var stackDelta = addInfo.AddStack;
            if (Stack == 0 && stackDelta == 0)
            {
                stackDelta = 1;
            }

            ModStack(stackDelta);
        }

        /// <summary>
        /// 生命周期是否结束
        /// </summary>
        public bool TickFinish(float deltaTime)
        {
            if (!Permanent)
            {
                Duration -= deltaTime;
                if (Duration < 0f)
                {
                    Duration = 0f;
                }
            }

            TimeElapsed += deltaTime;

            if (BuffData?.TickTimeInterval > 0f && BuffData.OnTick != null)
            {
                tickTime += deltaTime;
                while (tickTime >= BuffData.TickTimeInterval)
                {
                    tickTime -= BuffData.TickTimeInterval;
                    Ticked += 1;
                    BuffData.OnTick.Execute(this);
                }
            }

            return (!Permanent && Duration <= 0f) || Stack <= 0;
        }

        internal void ModDuration(float add, bool isOverrideDuration)
        {
            Duration = isOverrideDuration ? add : Duration + add;
            if (Duration < 0f)
            {
                Duration = 0f;
            }
        }

        /// <summary>
        /// 修改堆叠次数
        /// </summary>
        internal int ModStack(int add)
        {
            var finalStack = Stack + add;
            finalStack = Mathf.Clamp(finalStack, 0, GetMaxStack());

            var oldStack = Stack;
            Stack = finalStack;
            return finalStack - oldStack;
        }

        int GetMaxStack()
        {
            if (BuffData == null)
            {
                return 1;
            }

            return Mathf.Max(1, BuffData.MaxStack);
        }
    }
}
