using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// 共享缓冲区。收集EffectPipelineSystem的Request请求，各个System通过此缓冲区来提交Request。
    /// </summary>
    public class EffectRequestBuffer
    {
        List<EffectRequest> current;
        List<EffectRequest> pending;

        public EffectRequestBuffer(int initialCapacity = 32)
        {
            current = new(initialCapacity);
            pending = new(initialCapacity);
        }

        public void Submit(EffectRequest request)
        {
            current.Add(request);
        }

        public void SubmitDeferred(EffectRequest request)
        {
            pending.Add(request);
        }
        /// <summary>
        /// 获取本帧待处理的Request
        /// </summary>
        /// <returns></returns>
        public List<EffectRequest> GetCurrent()
        {
            return current;
        }

        public void Flush()
        {
            for (int i = 0; i < current.Count; i++)
            {
                // 回收
            }
            current.Clear();

            // 推进当前帧
            (current, pending) = (pending, current);
        }
    }
}