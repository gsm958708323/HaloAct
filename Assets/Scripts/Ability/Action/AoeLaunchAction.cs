using UnityEngine;

namespace Ability
{
    public class AoeLaunchAction : AbilityAction
    {
        public int aoe;

        protected override void OnEnter()
        {
            base.OnEnter();
            if (StartFrame != EndFrame)
            {
                Debugger.LogError($"连续多帧创建aoe ", LogDomain.Bullet);
                EndFrame = StartFrame;
            }

            var caster = tree?.Entity;
            if (caster is null)
            {
                return;
            }

            var transComp = caster.GetComp<TransfromComp>();
            if (transComp is null)
            {
                return;
            }

            var aoeData = FightManager.Config.LoadAoe(aoe);
            if (aoeData is null)
            {
                return;
            }

            var launcher = new AoeLauncher
            {
                AoeId = aoe,
                Data = aoeData,
                Caster = caster,
                Position = transComp.Position + transComp.Rotation * aoeData.MoveInfo.SpawnOffset,
                Velocity = transComp.Rotation * aoeData.MoveInfo.Velocity,
            };

            FightManager.LogicEntity.CreateAoe(launcher);
        }
    }
}
