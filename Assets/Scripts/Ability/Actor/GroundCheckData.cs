using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Ability
{
    [Serializable]
    public class GroundCheckData
    {
        /// <summary>
        /// x为检测半径，y为偏移量
        /// </summary>
        public Vector2 GroundSetting = new(0.5f, -0.7f);
        public LayerMask GroundLayer;
    }
}