using System.Reflection;
using NUnit.Framework;

namespace Ability.Tests
{
    public class BulletContractTests
    {
        [Test]
        public void BulletData_ContainsRuntimeFields()
        {
            var type = typeof(BulletData);
            Assert.NotNull(type.GetField("Speed", BindingFlags.Public | BindingFlags.Instance));
            Assert.NotNull(type.GetField("Duration", BindingFlags.Public | BindingFlags.Instance));
            Assert.NotNull(type.GetField("SpawnOffset", BindingFlags.Public | BindingFlags.Instance));
        }

        [Test]
        public void BulletAction_Execute_HasBulletAndTargetParameters()
        {
            var method = typeof(BulletAction).GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);

            var parameters = method.GetParameters();
            Assert.AreEqual(2, parameters.Length);
            Assert.AreEqual(typeof(BulletDataComp), parameters[0].ParameterType);
            Assert.AreEqual(typeof(Entity), parameters[1].ParameterType);
        }

        [Test]
        public void EventId_DefinesRemoveEntity()
        {
            var field = typeof(EventId).GetField("RemoveEntity", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(field);
        }
    }
}
