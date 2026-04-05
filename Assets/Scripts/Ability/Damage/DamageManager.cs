using System.Collections.Generic;
using HaloFrame;

namespace Ability
{
    public class DamageManager : IManager
    {
        readonly Queue<DamageInfo> queue = new();
        readonly Queue<DeferredBuffRequest> deferredBuffs = new();
        readonly List<EffectComp> dirtyEffects = new();
        readonly HashSet<EffectComp> dirtyEffectSet = new();

        public override int Priority => -5;

        public void Enqueue(DamageInfo info)
        {
            if (info == null || info.Attacker == null || info.Defender == null)
            {
                return;
            }

            queue.Enqueue(info);
        }

        public void Flush()
        {
            while (queue.Count > 0 || deferredBuffs.Count > 0)
            {
                while (queue.Count > 0)
                {
                    Resolve(queue.Dequeue());
                }

                ApplyDeferredBuffs();
            }
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            Flush();
        }

        void Resolve(DamageInfo info)
        {
            if (info.Attacker == null || info.Defender == null)
            {
                return;
            }

            info.Attacker.Target = info.Defender;
            info.Defender.Target = info.Attacker;

            info.Attacker.GetComp<EffectComp>()?.OnHitDamage(info);
            info.Defender.GetComp<EffectComp>()?.OnBeHurtDamage(info);

            CollectPendingBuffs(info);

            if (info.TriggerHurtBehavior)
            {
                TriggerHurtBehavior(info);
            }

            if (info.IsLethal)
            {
                ResolveLethal(info);
            }
        }

        void CollectPendingBuffs(DamageInfo info)
        {
            if (info.PendingBuffs == null || info.PendingBuffs.Count == 0)
            {
                return;
            }

            var effectComp = info.Defender.GetComp<EffectComp>();
            if (effectComp == null)
            {
                return;
            }

            for (int i = 0; i < info.PendingBuffs.Count; i++)
            {
                var addInfo = info.PendingBuffs[i];
                if (addInfo.Creater == null)
                {
                    addInfo.Creater = info.Attacker;
                }

                if (addInfo.Target == 0)
                {
                    addInfo.Target = info.Defender.Uid;
                }

                deferredBuffs.Enqueue(new DeferredBuffRequest(effectComp, addInfo));
            }
        }

        void ApplyDeferredBuffs()
        {
            if (deferredBuffs.Count == 0)
            {
                return;
            }

            while (deferredBuffs.Count > 0)
            {
                var request = deferredBuffs.Dequeue();
                if (request.EffectComp == null)
                {
                    continue;
                }

                request.EffectComp.EnqueueBuff(request.AddInfo);
                if (dirtyEffectSet.Add(request.EffectComp))
                {
                    dirtyEffects.Add(request.EffectComp);
                }
            }

            for (int i = 0; i < dirtyEffects.Count; i++)
            {
                dirtyEffects[i].FlushPending();
            }

            dirtyEffects.Clear();
            dirtyEffectSet.Clear();
        }

        void TriggerHurtBehavior(DamageInfo info)
        {
            if (info.Source is not AbilityBehaviorAttack attackBehavior)
            {
                return;
            }

            info.Defender.GetComp<AttackComp>()?.ResolveDamageHurt(info.Attacker, attackBehavior);
        }

        void ResolveLethal(DamageInfo info)
        {
            info.Defender.IsDead = true;
            info.Attacker.GetComp<EffectComp>()?.OnKillDamage(info);
            info.Defender.GetComp<EffectComp>()?.OnBeKilledDamage(info);
            info.Defender.DeathCheck();
        }

        readonly struct DeferredBuffRequest
        {
            public DeferredBuffRequest(EffectComp effectComp, AddBuffInfo addInfo)
            {
                EffectComp = effectComp;
                AddInfo = addInfo;
            }

            public EffectComp EffectComp { get; }
            public AddBuffInfo AddInfo { get; }
        }
    }
}
