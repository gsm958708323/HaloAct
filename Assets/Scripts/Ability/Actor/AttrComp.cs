using UnityEngine;

namespace Ability
{
    public class AttrComp : ComponentLogic
    {
        public ActorAttr BaseAttr;
        public ActorAttr RuntimePlusAttr;
        public ActorAttr RuntimeRatioAttr;
        public ActorAttr FinalAttr;

        public ActorControlState BaseControlState = ActorControlState.CreateDefault();
        public ActorControlState RuntimeControlState = ActorControlState.CreateDefault();
        public ActorControlState FinalControlState;

        public override void Enter(IEntity entity)
        {
            base.Enter(entity);
            LoadFromActorData();
            Recheck();
        }

        public void SetBaseAttr(ActorAttr attr)
        {
            BaseAttr = attr;
            Recheck();
        }

        public void SetBaseControlState(ActorControlState state)
        {
            BaseControlState = state;
            Recheck();
        }

        public void AddPlusAttr(ActorAttr attr)
        {
            RuntimePlusAttr += attr;
            Recheck();
        }

        public void AddRatioAttr(ActorAttr attr)
        {
            RuntimeRatioAttr += attr;
            Recheck();
        }

        public void SetRuntimeControlState(ActorControlState state)
        {
            RuntimeControlState = state;
            Recheck();
        }

        public void ResetRuntimeState()
        {
            RuntimePlusAttr = default;
            RuntimeRatioAttr = default;
            RuntimeControlState = ActorControlState.CreateDefault();
            Recheck();
        }

        public void Recheck()
        {
            FinalAttr = ApplyRatio(BaseAttr + RuntimePlusAttr, RuntimeRatioAttr);
            FinalControlState = MergeControlState(BaseControlState, RuntimeControlState);
        }

        void LoadFromActorData()
        {
            var data = entity?.GetComp<PlayerDataComp>()?.Data;
            if (data == null)
            {
                return;
            }

            BaseAttr = data.BaseAttr;
            // Movement still reads legacy physics fields from ActorData, so keep AttrComp in sync for now.
            BaseAttr.Gravity = data.Gravity;
            BaseAttr.DelayAerialTime = data.DelayAerialTime;
            BaseControlState = data.BaseControlState;
        }

        static ActorAttr ApplyRatio(ActorAttr baseAttr, ActorAttr ratioAttr)
        {
            return ActorAttr.LerpRatio(baseAttr, ratioAttr);
        }

        static ActorControlState MergeControlState(ActorControlState baseState, ActorControlState runtimeState)
        {
            return new ActorControlState
            {
                CanMove = baseState.CanMove && runtimeState.CanMove,
                CanRotate = baseState.CanRotate && runtimeState.CanRotate,
                CanUseSkill = baseState.CanUseSkill && runtimeState.CanUseSkill,
                CanAttack = baseState.CanAttack && runtimeState.CanAttack,
                CanBeControlled = baseState.CanBeControlled && runtimeState.CanBeControlled,
            };
        }
    }
}
