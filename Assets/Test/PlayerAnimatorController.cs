using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public static class AnimtorDefine
    {
        public const string Look = "Look";
        public const string Movement = "Movement";
        public const string HasInput = "HasInput";
        public const string Run = "Run";
    }

    public class PlayerAnimatorController : ILogic
    {
        Animator animator;

        public override void Bind<T>(T t)
        {
            base.Bind(t);
            this.animator = t as Animator;
        }

        public override void OnStart()
        {
            base.OnStart();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }

}