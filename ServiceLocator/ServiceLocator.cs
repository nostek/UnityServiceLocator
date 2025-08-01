using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityServiceLocator
{
	public static class ServiceLocator
	{
		static readonly Dictionary<object, object> services = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ClearStatics()
		{
			services?.Clear();
		}

		public static ServiceLookup Lookup { get; } = new();

		public static bool TryRegister<T>(T service) where T : class
		{
			if (service == null)
				return false;
			Register(service);
			return true;
		}

		public static T RegisterSingleton<T>() where T : class, new()
		{
			var service = TryGet<T>();
			if (service != null)
				return service;

			var instance = new T();
			Register(instance);
			return instance;
		}

		public static object RegisterSingleton(System.Type type, System.Func<object> factory)
		{
			var service = TryGet(type);
			if (service != null)
				return service;

			Assert.IsNotNull(factory);
			var instance = factory();
			Register(type, instance);
			return instance;
		}

		public static void Register<T>(T service) where T : class
		{
			Register(typeof(T), service);
		}

		public static void Register(System.Type type, object service)
		{
			Assert.IsNotNull(service, $"Provided service is null for {type}");
			Assert.IsFalse(services.ContainsKey(type), $"Service is already registered for {type}");
			services.Add(type, service);
			Debug.Log($"Service {type} registered");
		}

		public static void Unregister<T>()
		{
			Unregister(typeof(T));
		}

		public static void Unregister<T>(T service)
		{
			Unregister(typeof(T));
		}

		public static void Unregister(System.Type type)
		{
			Assert.IsTrue(services.ContainsKey(type), $"Service not registered for {type}");
			services.Remove(type);
			Debug.Log($"Service {type} unregistered");
		}

		public static T Get<T>()
		{
			if (!services.ContainsKey(typeof(T))) Assert.IsTrue(false, $"Service not registered for {typeof(T)}");
			return (T)services[typeof(T)];
		}

		public static T TryGet<T>()
		{
			if (services.TryGetValue(typeof(T), out var service))
				return (T)service;
			return default;
		}

		public static object TryGet(System.Type type)
		{
			if (services.TryGetValue(type, out var service))
				return service;
			return default;
		}
	}
}
