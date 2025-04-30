using NUnit.Framework;

namespace UnityServiceLocator
{
	internal class TestServiceLocator
	{
		[Test]
		public static void RunTestServiceLocator()
		{
			var to = new TestObject();

			Assert.IsNull(ServiceLocator.TryGet<TestObject>());
			Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ServiceLocator.Get<TestObject>());

			ServiceLocator.Register(to);
			Assert.IsNotNull(ServiceLocator.TryGet<TestObject>());

			Assert.AreEqual(ServiceLocator.TryGet<TestObject>(), to);

			ServiceLocator.Unregister(to);
			Assert.IsNull(ServiceLocator.TryGet<TestObject>());

			//

			TestObject toUndefined = null;

			Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ServiceLocator.Register<TestObject>(toUndefined));
			Assert.IsNull(ServiceLocator.TryGet<TestObject>());

			Assert.DoesNotThrow(() => ServiceLocator.TryRegister<TestObject>(toUndefined));
			Assert.IsNull(ServiceLocator.TryGet<TestObject>());

			//

			Assert.IsNull(ServiceLocator.TryGet<TestObject>());

			var singleton = ServiceLocator.RegisterSingleton<TestObject>();
			Assert.IsNotNull(singleton);

			var singleton1 = ServiceLocator.TryGet<TestObject>();
			Assert.IsNotNull(singleton1);

			Assert.AreEqual(singleton, singleton1);

			Assert.DoesNotThrow(() => ServiceLocator.RegisterSingleton<TestObject>());

			var singleton2 = ServiceLocator.TryGet<TestObject>();
			Assert.IsNotNull(singleton2);

			Assert.AreEqual(singleton1, singleton2);

			ServiceLocator.Unregister<TestObject>();
			Assert.IsNull(ServiceLocator.TryGet<TestObject>());
		}

		[Test]
		public static void RunTestServiceInstaller()
		{
			var to = new TestObject();
			TestObject toUndefined = null;

			Assert.IsNull(ServiceLocator.TryGet<TestObject>());

			var installer = new ServiceInstaller();

			Assert.DoesNotThrow(() => installer.TryRegister(toUndefined));
			Assert.IsNull(ServiceLocator.TryGet<TestObject>());

			Assert.DoesNotThrow(() => installer.Register(to));
			Assert.IsNotNull(ServiceLocator.TryGet<TestObject>());

			Assert.AreEqual(ServiceLocator.TryGet<TestObject>(), to);

			installer.Dispose();
			Assert.IsNull(ServiceLocator.TryGet<TestObject>());
		}

		[Test]
		public static void RunTestServiceLookup()
		{
			var to = new TestObject();

			Assert.IsNull(ServiceLocator.TryGet<TestObject>());

			ServiceLocator.Register(to);
			Assert.IsNotNull(ServiceLocator.TryGet<TestObject>());

			ServiceLocator.Lookup
				.Get(out TestObject outTo)
				.Done();

			Assert.IsNotNull(outTo);

			Assert.AreEqual(to, outTo);

			ServiceLocator.Unregister(to);
			Assert.IsNull(ServiceLocator.TryGet<TestObject>());
		}

		class TestObject
		{
		}
	}
}
