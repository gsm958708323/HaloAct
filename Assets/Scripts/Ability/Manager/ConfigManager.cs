using System;
using Ability;
using Frame;
using UnityEngine;


public class ConfigManager : IManager
{
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        var cfg = Resources.Load(path);
        if (cfg is null)
        {
            Debugger.LogError($"r配置不存在 {path}", LogDomain.Config);
            return null;
        }

        return (T)cfg;
    }

    public ActorData LoadActor(int actorId)
    {
        return Load<ActorData>($"Actor/{actorId}");
    }

    public BuffBehavior LoadBuff(int buffId)
    {
        return Load<BuffBehavior>($"Buff/{buffId}");
    }
}

