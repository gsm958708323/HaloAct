using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ability.Config
{
    public class CombatConfigValidationTests
    {
        readonly List<Object> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void Validate_RejectsMissingIdsAndNullTickCallbacks()
        {
            var invalidBuff = CreateBuffData(0);
            invalidBuff.Name = "InvalidBuff";
            invalidBuff.MaxStack = 1;
            invalidBuff.Modifier = BuffModifierGroup.CreateDefault();

            var invalidBullet = CreateBulletData(2001);
            invalidBullet.name = "InvalidBullet";
            invalidBullet.Duration = 0f;

            var invalidAoe = CreateAoeData(3001);
            invalidAoe.name = "InvalidAoe";
            invalidAoe.TickInterval = 0.25f;
            invalidAoe.OnTick = null;

            var report = ValidateAssets(invalidBuff, invalidBullet, invalidAoe);
            var errors = GetErrors(report);

            Assert.That(errors, Has.Count.EqualTo(3));
            Assert.That(errors, Has.Some.Contains("BuffData"));
            Assert.That(errors, Has.Some.Contains("BulletData"));
            Assert.That(errors, Has.Some.Contains("AoeData"));
        }

        [Test]
        public void Validate_RejectsDuplicateIdsWithinSameAssetType()
        {
            var firstBuff = CreateBuffData(1001);
            var secondBuff = CreateBuffData(1001);

            var report = ValidateAssets(firstBuff, secondBuff);
            var errors = GetErrors(report);

            Assert.That(errors, Has.Some.Contains("duplicate BuffData id: 1001"));
        }

        [Test]
        public void Validate_AcceptsMinimalValidCombatAssets()
        {
            var validBuff = CreateBuffData(1001);
            validBuff.Name = "ValidBuff";
            validBuff.MaxStack = 1;
            validBuff.Modifier = BuffModifierGroup.CreateDefault();

            var validBullet = CreateBulletData(2001);
            validBullet.Duration = 1f;
            validBullet.Radius = 0.1f;
            validBullet.HitTimes = 1;

            var validAoe = CreateAoeData(3001);
            validAoe.Duration = 1f;
            validAoe.Radius = 1f;
            validAoe.TickInterval = 0f;

            var report = ValidateAssets(validBuff, validBullet, validAoe);
            var errors = GetErrors(report);

            Assert.That(IsValid(report), Is.True);
            Assert.That(errors, Is.Empty);
        }

        BuffData CreateBuffData(int id)
        {
            var data = ScriptableObject.CreateInstance<BuffData>();
            data.Id = id;
            createdObjects.Add(data);
            return data;
        }

        BulletData CreateBulletData(int id)
        {
            var data = ScriptableObject.CreateInstance<BulletData>();
            data.Id = id;
            data.Duration = 1f;
            data.HitTimes = 1;
            data.Radius = 0.1f;
            createdObjects.Add(data);
            return data;
        }

        AoeData CreateAoeData(int id)
        {
            var data = ScriptableObject.CreateInstance<AoeData>();
            data.Id = id;
            data.Duration = 1f;
            data.Radius = 1f;
            createdObjects.Add(data);
            return data;
        }

        static object ValidateAssets(params ScriptableObject[] assets)
        {
            var validatorType = Type.GetType("Ability.CombatDataValidator, Assembly-CSharp-Editor");
            Assert.That(validatorType, Is.Not.Null, "CombatDataValidator type is missing.");

            var validateMethod = validatorType.GetMethod(
                "Validate",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IEnumerable<ScriptableObject>) },
                null);
            Assert.That(validateMethod, Is.Not.Null, "CombatDataValidator.Validate is missing.");

            return validateMethod.Invoke(null, new object[] { assets });
        }

        static IList<string> GetErrors(object report)
        {
            Assert.That(report, Is.Not.Null, "Validation report is missing.");

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            var field = report.GetType().GetField("Errors", flags);
            if (field?.GetValue(report) is IList<string> fieldErrors)
            {
                return fieldErrors;
            }

            var property = report.GetType().GetProperty("Errors", flags);
            if (property?.GetValue(report) is IList<string> propertyErrors)
            {
                return propertyErrors;
            }

            Assert.Fail("Validation report does not expose Errors.");
            return Array.Empty<string>();
        }

        static bool IsValid(object report)
        {
            Assert.That(report, Is.Not.Null, "Validation report is missing.");

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            var property = report.GetType().GetProperty("IsValid", flags);
            if (property != null)
            {
                return (bool)property.GetValue(report);
            }

            var errors = GetErrors(report);
            return errors.Count == 0;
        }
    }
}
