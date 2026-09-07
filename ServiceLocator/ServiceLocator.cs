using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityServiceLocator
{
	public static partial class ServiceLocator
	{
		static readonly Dictionary<Type, object> services = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ClearStatics()
		{
			services?.Clear();
		}

		public static bool TryRegister<T>(T service) where T : class
		{
			if (service == null)
				return false;
			Register(service);
			return true;
		}

		public static T RegisterSingleton<T>() where T : class, new()
		{
			if (TryGet(out T service))
				return service;

			var instance = new T();
			Register(instance);
			return instance;
		}

		public static object RegisterSingleton(Type type, Func<object> factory)
		{
			if (TryGet(out object service))
				return service;

			Assert.IsNotNull(factory);
			var instance = factory();
			Register(type, instance);
			return instance;
		}

		public static object RegisterSingletonAs(Type objectType, Type interfaceType, Func<object> factory)
		{
			if (TryGet(out object service))
				return service;

			Assert.IsNotNull(factory);
			var instance = factory();
			Register(objectType, instance);
			Register(interfaceType, instance);
			return instance;
		}

		public static void Register<T>(T service) where T : class
		{
			Register(typeof(T), service);
		}

		public static void Register(Type type, object service)
		{
			Assert.IsNotNull(service, $"Provided service is null for {type}");
			Assert.IsFalse(services.ContainsKey(type), $"Service is already registered for {type}");
			services.Add(type, service);
			Debug.Log($"Service {type} registered");
			OnChanged?.Invoke(type, service);
		}

		public static void Unregister<T>()
		{
			Unregister(typeof(T));
		}

		public static void Unregister<T>(T service)
		{
			Unregister(typeof(T));
		}

		public static void Unregister(Type type)
		{
			Assert.IsTrue(services.ContainsKey(type), $"Service not registered for {type}");
			services.Remove(type);
			Debug.Log($"Service {type} unregistered");
			OnChanged?.Invoke(type, null);
		}

		public static T Get<T>()
		{
			if (!services.ContainsKey(typeof(T))) Assert.IsTrue(false, $"Service not registered for {typeof(T)}");
			return (T)services[typeof(T)];
		}

		public static T GetOrDefault<T>()
		{
			if (services.TryGetValue(typeof(T), out var service))
				return (T)service;
			return default;
		}

		public static object GetOrDefault(Type type)
		{
			return services.GetValueOrDefault(type);
		}

		public static bool TryGet<T>(out T service)
		{
			if (services.TryGetValue(typeof(T), out var registered))
			{
				service = (T)registered;
				return true;
			}
			service = default;
			return false;
		}

		public static bool TryGet(Type type, out object service)
		{
			return services.TryGetValue(type, out service);
		}

		#region LOOKUP

		private static readonly Lookup lookup = new();

		public static Lookup Get<T>(out T service) => lookup.Get(out service);

		public static Lookup GetOrDefault<T>(out T service) => lookup.GetOrDefault(out service);

		#endregion

		#region EVENT

		public delegate void OnChangedDelegate(Type type, object service);

		public static event OnChangedDelegate OnChanged;

		#endregion

		#region AWAITABLE

		public static async Awaitable<T> WaitForAsync<T>(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				if (TryGet(out T service))
					return service;

				await Awaitable.NextFrameAsync();
			}

			cancellationToken.ThrowIfCancellationRequested();

			return default;
		}

		#endregion
	}
}
