using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
public enum LogDomain
{
    None, All, Project_Setup, ActorData, ActorControl, ActorBehavior, ActorFSM, BehaviorAcrion, FrameInfo,
    BehaviorRequirement,
    BT_Conditional,
    DataSerialization,
    Input
}

public class Debugger : MonoSingleton<Debugger>
{
    [ShowInInspector]
    public static LogDomain logDomain = LogDomain.All;
    /// <summary>
    /// Log Messages
    /// </summary>
    /// <param name="level"></param>
    /// <param name="msg"></param>
    public static void Log(string msg, LogDomain domain)
    {
        if (logDomain == LogDomain.None)
        {
            return;
        }
        else if (logDomain == LogDomain.All)
        {
            Debug.Log($"<b><color=#008AFF>[{domain}]: </color></b>" + msg);
        }
        else if (domain == logDomain)
        {
            Debug.Log($"<b><color=#008AFF>[{domain}]: </color></b>" + msg);
        }
    }

    /// <summary>
    /// Log Warnings
    /// </summary>
    /// <param name="level"></param>
    /// <param name="msg"></param>
    public static void LogWarning(string msg, LogDomain domain)
    {
        if (logDomain == LogDomain.None)
        {
            return;
        }
        else if (logDomain == LogDomain.All)
        {
            Debug.LogWarning($"<b><color=#008AFF>[{domain}]: </color></b>" + msg);
        }
        else if (domain == logDomain)
        {
            Debug.LogWarning($"<b><color=#008AFF>[{domain}]: </color></b>" + msg);
        }
    }

    /// <summary>
    /// Log Errors
    /// </summary>
    /// <param name="level"></param>
    /// <param name="msg"></param>
    public static void LogError(string msg, LogDomain domain)
    {
        if (logDomain == LogDomain.None)
        {
            return;
        }
        else if (logDomain == LogDomain.All)
        {
            Debug.LogError($"<b><color=#008AFF>[{domain}]: </color></b>" + msg);
        }
        else if (domain == logDomain)
        {
            Debug.LogError($"<b><color=#008AFF>[{domain}]: </color></b>" + msg);
        }
    }
}
