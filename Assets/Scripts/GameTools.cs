using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public static class GameTools
{
    /// <summary>
    /// 绑定轨道目标
    /// </summary>
    public static bool BindTrackObject(PlayableDirector playableDirector, string trackName, Object value)
    {
        foreach (PlayableBinding playableAssetOutput in playableDirector.playableAsset.outputs)
        {
            if (playableAssetOutput.streamName == trackName)
            {
                playableDirector.SetGenericBinding(playableAssetOutput.sourceObject, value);
                return true;
            }
        }

        return false;
    }

    public static int GetHitAbilityId(int id)
    {
        return id + 100;
    }
}
