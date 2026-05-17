using System;
using System.Collections.Generic;

namespace Combat
{
public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : struct;
        void Unsubscribe<T>(Action<T> handler) where T : struct;
        void Publish<T>(T evt) where T : struct;
    }

    public class EventBus : IEventBus
    {
        // 按事件类型存储订阅者
        // 值类型是 object，实际存 Action<T>
        private readonly Dictionary<Type, List<Delegate>> handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                handlers[type] = list;
            }

            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!handlers.TryGetValue(type, out var list)) return;
            list.Remove(handler);
        }

        public void Publish<T>(T evt) where T : struct
        {
            var type = typeof(T);
            if (!handlers.TryGetValue(type, out var list)) return;

            // 倒序遍历，防止 handler 内部 Unsubscribe 导致索引错乱
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] is Action<T> action)
                {
                    action.Invoke(evt);
                }
            }
        }
    }
}