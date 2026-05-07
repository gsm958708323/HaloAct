using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class ConfigManager
    {
        private readonly Dictionary<int, BuffConfig> buffConfigs = new();
        private readonly Dictionary<int, BulletConfig> bulletConfigs = new();
        private readonly Dictionary<int, AoeConfig> aoeConfigs = new();

        public BuffConfig GetBuffConfig(int id) =>
            buffConfigs.TryGetValue(id, out var cfg) ? cfg : null;

        public BulletConfig GetBulletConfig(int id) =>
            bulletConfigs.TryGetValue(id, out var cfg) ? cfg : null;

        public AoeConfig GetAOEConfig(int id) =>
            aoeConfigs.TryGetValue(id, out var cfg) ? cfg : null;

        public void Register(BuffConfig cfg) => buffConfigs[cfg.BuffId] = cfg;
        public void Register(BulletConfig cfg) => bulletConfigs[cfg.BulletId] = cfg;
        public void Register(AoeConfig cfg) => aoeConfigs[cfg.AoeId] = cfg;

        /// <summary>
        /// 从 Resources 加载所有 ScriptableObject 配置。
        /// 商用项目替换为 Addressables。
        /// </summary>
        public void LoadAll()
        {
            foreach (var cfg in Resources.LoadAll<BuffConfig>("Configs/Buffs"))
                Register(cfg);
            foreach (var cfg in Resources.LoadAll<BulletConfig>("Configs/Bullets"))
                Register(cfg);
            foreach (var cfg in Resources.LoadAll<AoeConfig>("Configs/AOEs"))
                Register(cfg);
        }
    }
}