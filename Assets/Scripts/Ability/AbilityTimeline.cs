using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public enum AbilityState
{
    None,
    Playing,
    Completed,
    Canceled,
}

public class AbilityTimeline : MonoBehaviour
{
    public Action CompleteCB;
    public Action ComboCB;
    public AbilityState State;

    private PlayableDirector playableDirector;
    AbilityData abilityData;

    public void Init(AbilityData data, PlayerAbilityController controller)
    {
        abilityData = data;
        playableDirector = GetComponent<PlayableDirector>();
        GameTools.BindTrackObject(playableDirector, "Animation Track", controller.GetComponentInChildren<Animator>());
        gameObject.transform.SetParent(controller.gameObject.transform);

        //技能碰撞回调
        HitBox hitBox = GetComponentInChildren<HitBox>(true);
        hitBox.AddTriggerCB(OnTrigger);
    }

    private void OnTrigger(PlayerAbilityController obj)
    {
        // 受击timeline
        int hitAbilityId = GameTools.GetHitAbilityId(abilityData.AbilityResId);
        AbilityTimeline timeline = ResMgr.Instance.GetAbility(hitAbilityId);
        timeline.Execute();
    }

    public void Execute(Action comboCB = null)
    {
        if (State == AbilityState.Playing)
            return;
        print("技能开始");
        State = AbilityState.Playing;
        playableDirector.Play();
        comboCB?.Invoke();
    }

    public void Cancel()
    {
        State = AbilityState.Canceled;
        playableDirector.Stop();
    }

    public void Complete()
    {
        print("技能完成");
        State = AbilityState.Completed;
        CompleteCB?.Invoke();
    }

    public void Combo()
    {
        print("技能连招");
        ComboCB?.Invoke();
    }
}
