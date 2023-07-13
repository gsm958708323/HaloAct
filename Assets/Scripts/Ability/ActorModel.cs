using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using static GameInput;

namespace Ability
{
    public enum ActorType
    {
        PLAYER,
        Enemy,
    }

    public class ActorModel : MonoBehaviour
    {
        [FolderPath] public string NodePath;
        [FolderPath] public string BehaviorPath;
        [HideInInspector] public AbilityBehaviorTree tree;
        [HideInInspector] public HitBox HitBox;
        [HideInInspector] public HurtBox HurtBox;
        public ActorType ActorType;
        public ActorModel Target;

        /// <summary>
        /// 缓存时间，用于计算帧数
        /// </summary>
        private float cacheTime;
        /// <summary>
        /// 当前运行的帧数
        /// </summary>
        private int curFrame;
        private float fps;

        public bool IsDead { get; internal set; }
        public bool IsInvincible { get; internal set; }

        public float Gravity = -9.8f;
        public float RotationAngle;
        public Vector2 InputDir;
        public Vector3 Velocity;
        CharacterController characterController;
        public PlayerGameInput GameInput;

        private void Awake()
        {
            fps = 1.0f / GameManager_Settings.TargetFraneRate;
            curFrame = 1;
        }

        void Start()
        {
            tree = new AbilityBehaviorTree();
            tree.Init(NodePath, BehaviorPath);
            tree.Enter(this);

            HitBox = GetComponentInChildren<HitBox>();
            HitBox.Init();
            HitBox.Exit(); // 默认隐藏

            HurtBox = GetComponentInChildren<HurtBox>();
            HurtBox.Init();
            HurtBox.Enter(this);

            characterController = GetComponent<CharacterController>();
            GameInput = GetComponent<PlayerGameInput>();
        }

        void Update()
        {
            cacheTime += Time.deltaTime;

            // 超过fps执行一次Tick
            while (cacheTime > fps)
            {
                tree.Tick(curFrame);
                curFrame += 1;
                cacheTime -= fps;
            }

            UpdatePhysics();
        }

        private void UpdatePhysics()
        {
            characterController.Move(Velocity * Time.deltaTime);
            Velocity.y += Gravity * Time.deltaTime;
            Velocity.y = Mathf.Clamp(Velocity.y, -20, 50);
        }

        public PlayerInputActions GetPlayerInput()
        {
            return GameInput.PlayerAction.PlayerInput;
        }

        internal void DeathCheck()
        {

        }
    }
}