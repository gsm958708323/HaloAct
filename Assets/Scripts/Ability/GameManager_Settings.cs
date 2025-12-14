using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class GameManager_Settings : MonoSingleton<GameManager_Settings>
    {
        public static int TargetFraneRate = 60;

        override protected void Awake()
        {
            base.Awake();
            Application.targetFrameRate = TargetFraneRate;
        }

        private void Update()
        {
            if (Application.targetFrameRate != TargetFraneRate)
            {
                Application.targetFrameRate = TargetFraneRate;
            }
        }
    }
}

