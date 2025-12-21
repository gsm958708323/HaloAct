using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class BulletModel : ILogicT<BulletData>
    {
        public void Enter(BulletData t)
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void Tick(float deltaTime)
        {
            throw new System.NotImplementedException();
        }
    }

    public struct BulletLauncher
    {
        public int BulletId;
        public string Prefab;
    }
}
