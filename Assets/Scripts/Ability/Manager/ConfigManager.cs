using System;
using Ability;
using HaloFrame;
using UnityEngine;


public class ConfigManager : IManager
{
    public const string ActorResourceFolder = "Actor";
    public const string BuffResourceFolder = "Buff";
    public const string BulletResourceFolder = "Bullet";
    public const string AoeResourceFolder = "Aoe";

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

    public static string GetActorResourcePath(int actorId)
    {
        return $"{ActorResourceFolder}/{actorId}";
    }

    public static string GetBuffResourcePath(int buffId)
    {
        return $"{BuffResourceFolder}/{buffId}";
    }

    public static string GetBulletResourcePath(int bulletId)
    {
        return $"{BulletResourceFolder}/{bulletId}";
    }

    public static string GetAoeResourcePath(int aoeId)
    {
        return $"{AoeResourceFolder}/{aoeId}";
    }

    public ActorData LoadActor(int actorId)
    {
        return Load<ActorData>(GetActorResourcePath(actorId));
    }

    public BuffData LoadBuff(int buffId)
    {
        return Load<BuffData>(GetBuffResourcePath(buffId));
    }

    internal BulletData LoadBullet(int bulletId)
    {
        return Load<BulletData>(GetBulletResourcePath(bulletId));
    }

    internal AoeData LoadAoe(int aoeId)
    {
        return Load<AoeData>(GetAoeResourcePath(aoeId));
    }
}
