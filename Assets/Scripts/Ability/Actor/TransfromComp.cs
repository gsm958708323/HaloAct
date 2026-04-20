using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class TransfromComp : ComponentLogic
    {
        /// <summary>
        /// 外部不能直接设置位置，通过设置Velocity来改变位置 
        /// </summary>
        public Vector3 Position { set; get; }
        public Quaternion Rotation;
        public Vector3 forward
        {
            get
            {
                return Rotation * Vector3.forward;
            }
        }
        public Vector3 right
        {
            get
            {
                return Rotation * Vector3.right;
            }
        }
        public Vector3 up
        {
            get
            {
                return Rotation * Vector3.up;
            }
        }

        public void SetBornPos(BornPosInfo bornInfo)
        {
            if (bornInfo.BornPosEnum == BornPosEnum.FixedPosition)
            {
                Position = bornInfo.Pos;
            }
        }
    }
}
