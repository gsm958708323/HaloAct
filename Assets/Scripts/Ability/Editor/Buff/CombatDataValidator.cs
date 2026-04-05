using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public sealed class CombatDataValidationResult
    {
        public readonly List<string> Errors = new();
        public readonly List<string> Warnings = new();

        public bool IsValid => Errors.Count == 0;

        public void AddError(string message)
        {
            Errors.Add(message);
        }

        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }
    }

    public static class CombatDataValidator
    {
        public static CombatDataValidationResult Validate(IEnumerable<ScriptableObject> assets)
        {
            var result = new CombatDataValidationResult();
            if (assets == null)
            {
                result.AddError("combat config collection is missing");
                return result;
            }

            var idRegistry = new Dictionary<Type, HashSet<int>>();
            foreach (var asset in assets)
            {
                if (asset == null)
                {
                    result.AddError("combat config asset is null");
                    continue;
                }

                switch (asset)
                {
                    case BuffData buff:
                        ValidateBuff(buff, result, idRegistry);
                        break;
                    case BulletData bullet:
                        ValidateBullet(bullet, result, idRegistry);
                        break;
                    case AoeData aoe:
                        ValidateAoe(aoe, result, idRegistry);
                        break;
                    default:
                        result.AddWarning($"unsupported combat config type: {asset.GetType().Name}");
                        break;
                }
            }

            return result;
        }

        static void ValidateBuff(BuffData buff, CombatDataValidationResult result, Dictionary<Type, HashSet<int>> idRegistry)
        {
            if (!ValidatePositiveId(buff, buff.Id, result, idRegistry))
            {
                return;
            }

            if (buff.MaxStack <= 0)
            {
                result.AddError($"{DescribeAsset(buff)} max stack must be greater than 0");
            }

            if (buff.TickTimeInterval < 0f)
            {
                result.AddError($"{DescribeAsset(buff)} tick interval must be >= 0");
            }
            else if (buff.HasTimedTick && buff.OnTick == null)
            {
                result.AddError($"{DescribeAsset(buff)} requires OnTick when TickTimeInterval > 0");
            }
        }

        static void ValidateBullet(BulletData bullet, CombatDataValidationResult result, Dictionary<Type, HashSet<int>> idRegistry)
        {
            if (!ValidatePositiveId(bullet, bullet.Id, result, idRegistry))
            {
                return;
            }

            if (!bullet.HasValidLifetime)
            {
                result.AddError($"{DescribeAsset(bullet)} duration must be greater than 0");
            }

            if (!bullet.HasValidHitTimes)
            {
                result.AddError($"{DescribeAsset(bullet)} hit times must be greater than 0");
            }

            if (bullet.Radius < 0f)
            {
                result.AddError($"{DescribeAsset(bullet)} radius must be >= 0");
            }

            if (bullet.CanHitAfterCreated < 0f)
            {
                result.AddError($"{DescribeAsset(bullet)} can-hit-after-created must be >= 0");
            }

            if (bullet.HitSameDelay < 0f)
            {
                result.AddError($"{DescribeAsset(bullet)} hit-same-delay must be >= 0");
            }
        }

        static void ValidateAoe(AoeData aoe, CombatDataValidationResult result, Dictionary<Type, HashSet<int>> idRegistry)
        {
            if (!ValidatePositiveId(aoe, aoe.Id, result, idRegistry))
            {
                return;
            }

            if (aoe.Duration <= 0f)
            {
                result.AddError($"{DescribeAsset(aoe)} duration must be greater than 0");
            }

            if (aoe.Radius <= 0f)
            {
                result.AddError($"{DescribeAsset(aoe)} radius must be greater than 0");
            }

            if (aoe.TickInterval < 0f)
            {
                result.AddError($"{DescribeAsset(aoe)} tick interval must be >= 0");
            }
            else if (aoe.HasTimedTick && aoe.OnTick == null)
            {
                result.AddError($"{DescribeAsset(aoe)} requires OnTick when TickInterval > 0");
            }
        }

        static bool ValidatePositiveId(
            ScriptableObject asset,
            int id,
            CombatDataValidationResult result,
            Dictionary<Type, HashSet<int>> idRegistry)
        {
            var type = asset.GetType();
            if (id <= 0)
            {
                result.AddError($"{type.Name} {GetAssetName(asset)} has invalid id: {id}");
                return false;
            }

            if (!idRegistry.TryGetValue(type, out var usedIds))
            {
                usedIds = new HashSet<int>();
                idRegistry[type] = usedIds;
            }

            if (!usedIds.Add(id))
            {
                result.AddError($"duplicate {type.Name} id: {id}");
                return false;
            }

            return true;
        }

        static string DescribeAsset(ScriptableObject asset)
        {
            return $"{asset.GetType().Name} {GetAssetName(asset)}";
        }

        static string GetAssetName(ScriptableObject asset)
        {
            return string.IsNullOrWhiteSpace(asset.name) ? "<unnamed>" : asset.name;
        }
    }
}
