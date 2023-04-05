using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class AbilityTimeline : MonoBehaviour
{
    private PlayableDirector playableDirector;
    AbilityData abilityData;

    public void Init(AbilityData data, PlayerTimelineController controller)
    {
        abilityData = data;
        playableDirector = GetComponent<PlayableDirector>();
        GameTools.BindTrackObject(playableDirector, "Animation Track", controller.GetComponentInChildren<Animator>());
        gameObject.transform.SetParent(controller.gameObject.transform);

        //绑定技能按键
        if (data.BindingKey != KeyCode.None)
        {
            InputMgr.Instance.AddKeyDown(data.BindingKey, (keyCode) =>
            {
                this.Execute();
            });
        }

        //技能碰撞回调
        HitBox hitBox = GetComponentInChildren<HitBox>(true);
        hitBox.AddTriggerCB(OnTrigger);
    }

    private void OnTrigger(PlayerTimelineController obj)
    {
        // 受击timeline
        int hitAbilityId = GameTools.GetHitAbilityId(abilityData.AbilityResId);
        AbilityTimeline timeline = ResMgr.Instance.GetAbility(hitAbilityId);
        timeline.Execute();
    }

    public void Execute()
    {
        print("技能开始");
        playableDirector.Play();
    }

    public void Cancel()
    {
        playableDirector.Stop();
    }

    public void Complete()
    {
        print("技能完成");
    }


}
