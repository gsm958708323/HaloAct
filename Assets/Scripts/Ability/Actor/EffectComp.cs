using System.Collections.Generic;

namespace Ability
{
    public class EffectComp : ComponentLogic
    {
        readonly Queue<AddBuffInfo> pending = new();
        readonly List<EffectObj> removeList = new();

        List<EffectObj> buffList;
        Dictionary<int, EffectObj> buffDict;

        public override void Init()
        {
            buffList = new();
            buffDict = new();
            pending.Clear();
            removeList.Clear();
        }

        public override void Destroy()
        {
            pending.Clear();
            removeList.Clear();
            buffList = null;
            buffDict = null;
            base.Destroy();
        }

        public override void Tick(float deltaTime)
        {
            if (entity.IsDead)
            {
                return;
            }

            removeList.Clear();
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                if (buff.TickFinish(deltaTime))
                {
                    removeList.Add(buff);
                }
            }

            var removedAny = removeList.Count > 0;
            for (int i = 0; i < removeList.Count; i++)
            {
                RemoveBuff(removeList[i], false);
            }

            if (pending.Count > 0)
            {
                var appliedAny = FlushPending();
                if (removedAny && !appliedAny)
                {
                    AttrRecheck();
                }
            }
            else if (removedAny)
            {
                AttrRecheck();
            }
        }

        public AbilityNode OnStartBehavior(AbilityNode node)
        {
            AbilityNode newNode = null;
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                if (buff.BuffData.OnCast != null)
                {
                    newNode = buff.BuffData.OnCast.Execute(buff, node);
                }
            }
            return newNode;
        }

        public void EnqueueBuff(AddBuffInfo addInfo)
        {
            pending.Enqueue(addInfo);
        }

        public bool FlushPending()
        {
            var dirty = false;
            while (pending.Count > 0)
            {
                dirty |= ApplyBuff(pending.Dequeue());
            }

            if (dirty)
            {
                AttrRecheck();
            }

            return dirty;
        }

        public EffectObj AddBuff(AddBuffInfo addInfo)
        {
            EnqueueBuff(addInfo);
            FlushPending();
            var buffId = addInfo.Data != null ? addInfo.Data.Id : addInfo.BuffId;
            return GetBuff(buffId);
        }

        public EffectObj GetBuff(int buffId)
        {
            buffDict.TryGetValue(buffId, out var buff);
            return buff;
        }

        public void OnHitDamage(DamageInfo damage)
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                buff.BuffData.OnHit?.Execute(buff, damage);
            }
        }

        public void OnBeHurtDamage(DamageInfo damage)
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                buff.BuffData.OnBeHurt?.Execute(buff, damage);
            }
        }

        public void OnKillDamage(DamageInfo damage)
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                buff.BuffData.OnKill?.Execute(buff, damage);
            }
        }

        public void OnBeKilledDamage(DamageInfo damage)
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                buff.BuffData.OnBeKilled?.Execute(buff, damage);
            }
        }

        bool ApplyBuff(AddBuffInfo addInfo)
        {
            var buffData = addInfo.Data ?? FightManager.Config.LoadBuff(addInfo.BuffId);
            if (buffData == null)
            {
                return false;
            }

            var buff = GetBuff(buffData.Id);
            if (buff == null)
            {
                buff = new EffectObj();
                buff.Init();
                buff.Enter(buffData);

                buffList.Add(buff);
                buffList.Sort((a, b) =>
                {
                    return a.BuffData.Priority.CompareTo(b.BuffData.Priority);
                });
                buffDict[buffData.Id] = buff;
            }

            buff.Apply(entity, addInfo);
            if (buff.Stack <= 0 || (!buff.Permanent && buff.Duration <= 0f))
            {
                RemoveBuff(buff, false);
                return true;
            }

            buffData.OnOccur?.Execute(buff);
            return true;
        }

        /// <summary>
        /// 重新计算所有属性
        /// </summary>
        void AttrRecheck()
        {
            var attrComp = entity.GetComp<AttrComp>();
            if (attrComp == null)
            {
                return;
            }

            var totalPlus = default(ActorAttr);
            var totalRatio = default(ActorAttr);
            var controlState = ActorControlState.CreateDefault();

            for (int i = 0; i < buffList.Count; i++)
            {
                var buff = buffList[i];
                var modifier = buff.BuffData.Modifier;
                totalPlus += modifier.GetStackedPlus(buff.Stack);
                totalRatio += modifier.GetStackedRatio(buff.Stack);
                controlState = MergeControlState(controlState, modifier.ControlStateMod);
            }

            attrComp.ResetRuntimeState();
            attrComp.AddPlusAttr(totalPlus);
            attrComp.AddRatioAttr(totalRatio);
            attrComp.SetRuntimeControlState(controlState);
        }

        static ActorControlState MergeControlState(ActorControlState currentState, ActorControlState buffState)
        {
            return new ActorControlState
            {
                CanMove = currentState.CanMove && buffState.CanMove,
                CanRotate = currentState.CanRotate && buffState.CanRotate,
                CanUseSkill = currentState.CanUseSkill && buffState.CanUseSkill,
                CanAttack = currentState.CanAttack && buffState.CanAttack,
                CanBeControlled = currentState.CanBeControlled && buffState.CanBeControlled,
            };
        }

        void RemoveBuff(EffectObj buff, bool recheck)
        {
            if (buff == null || buff.BuffData == null)
            {
                return;
            }

            buffList.Remove(buff);
            buffDict.Remove(buff.BuffData.Id);
            buff.BuffData.OnRemoved?.Execute(buff);
            buff.Exit();

            if (recheck)
            {
                AttrRecheck();
            }
        }
    }

    /// <summary>
    /// 存放添加buff时的动态数据
    /// </summary>
    public struct AddBuffInfo
    {
        /// <summary>
        /// buff配置id
        /// </summary>
        public int BuffId;
        public BuffData Data;
        /// <summary>
        /// buff创建者 todo
        /// </summary>
        public Entity Creater;
        /// <summary>
        /// buff目标
        /// </summary>
        public int Target;

        /// <summary>
        /// 修改堆叠次数，正负数
        /// </summary>
        public int AddStack { get; set; }
        /// <summary>
        /// 是否重写持续时间
        /// </summary>
        public bool IsOverrideDuration;

        /// <summary>
        /// 持续时间
        /// </summary>
        public float Duration { get; set; }
        /// <summary>
        /// 是否为永久型buff
        /// </summary>
        public bool Permanent { get; set; }
    }
}
