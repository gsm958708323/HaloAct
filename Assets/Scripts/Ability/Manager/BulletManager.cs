using System.Collections;
using System.Collections.Generic;
using HaloFrame;

namespace Ability
{
    public class BulletManager : IManager
    {
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            var list = FightManager.LogicEntity.GetEntityLinkedList(EntityType.Bullet);
            if (list is null)
                return;

            var node = list.First;
            while (node != null)
            {
                var bullet = node.Value;
                var comp = bullet.GetComp<BulletDataComp>();
                var data = comp.Data;

                if (comp is null)
                    continue;
                if (comp.Hp <= 0)
                    continue;
                // if (comp.TimeElapsed <= 0 && data.OnCreate != null)
                //     data.OnCreate.Execute(comp);

                node = node.Next;
            }
        }
    }
}

