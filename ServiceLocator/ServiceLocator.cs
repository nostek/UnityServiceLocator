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

			service = new T();
			Register(service);
			return service;
		}

		public static T RegisterSingleton<T>(T instance) where T : class
		{
			var service = TryGet<T>();
			if (service != null)
				return service;

			Assert.IsNotNull(instance);
			Register(instance);
			return instance;
		}

		public static void Register<T>(T service) where T : class
		{
			Register(typeof(T), service);
		}

		public static void Register(System.Type type, object service)
		{
			Assert.IsNotNull(service, $"Provieded service is null for {type}");
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
			Assert.IsTrue(services.ContainsKey(typeof(T)), $"Service not registered for {typeof(T)}");
			return (T)services[typeof(T)];
		}

		public static T TryGet<T>()
		{
			if (services.TryGetValue(typeof(T), out var service))
				return (T)service;
			return default;
		}
	}
}
