using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace UnityServiceLocator
{
	internal class TestServiceLocator
	{
		[Test]
		public static void RunTestServiceLocator()
		{
			var to = new TestObject();

			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());
			Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ServiceLocator.Get<TestObject>());

			ServiceLocator.Register(to);
			Assert.IsNotNull(ServiceLocator.GetOrDefault<TestObject>());

			Assert.AreEqual(ServiceLocator.Get<TestObject>(), to);

			Assert.That(() =>
			{
				ServiceLocator.Get<TestObject>();
			}, Is.Not.AllocatingGCMemory());

			ServiceLocator.Unregister(to);
			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());

			//

			TestObject toUndefined = null;

			Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ServiceLocator.Register<TestObject>(toUndefined));
			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());

			Assert.DoesNotThrow(() => ServiceLocator.TryRegister<TestObject>(toUndefined));
			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());

			//

			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());

			var singleton = ServiceLocator.RegisterSingleton<TestObject>();
			Assert.IsNotNull(singleton);

			var singleton1 = ServiceLocator.GetOrDefault<TestObject>();
			Assert.IsNotNull(singleton1);

			Assert.AreEqual(singleton, singleton1);

			Assert.DoesNotThrow(() => ServiceLocator.RegisterSingleton<TestObject>());

			var singleton2 = ServiceLocator.GetOrDefault<TestObject>();
			Assert.IsNotNull(singleton2);

			Assert.AreEqual(singleton1, singleton2);

			ServiceLocator.Unregister<TestObject>();
			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());
		}

		[Test]
		public static void RunTestServiceInstaller()
		{
			var to = new TestObject();
			TestObject toUndefined = null;

			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());

			var installer = new ServiceInstaller();

			Assert.DoesNotThrow(() => installer.TryRegister(toUndefined));
			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());

			Assert.DoesNotThrow(() => installer.Register(to));
			Assert.IsNotNull(ServiceLocator.GetOrDefault<TestObject>());

			Assert.AreEqual(ServiceLocator.GetOrDefault<TestObject>(), to);

			installer.Dispose();
			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());
		}

		[Test]
		public static void RunTestServiceLookup()
		{
			var to = new TestObject();

			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());

			ServiceLocator.Register(to);
			Assert.IsNotNull(ServiceLocator.GetOrDefault<TestObject>());

			ServiceLocator
				.Get(out TestObject outTo)
				.Done();

			Assert.IsNotNull(outTo);

			Assert.AreEqual(to, outTo);

			ServiceLocator.Unregister(to);
			Assert.IsNull(ServiceLocator.GetOrDefault<TestObject>());
		}

		class TestObject
		{
		}
	}
}
