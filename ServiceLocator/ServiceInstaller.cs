using System;
using System.Collections.Generic;

namespace UnityServiceLocator
{
	public class ServiceInstaller : IDisposable
	{
		readonly List<Type> services = new();

		public ServiceInstaller RegisterSingleton<T>() where T : class, new()
		{
			ServiceLocator.RegisterSingleton<T>();
			return this;
		}

		public ServiceInstaller RegisterSingleton<T>(out T service) where T : class, new()
		{
			service = ServiceLocator.RegisterSingleton<T>();
			return this;
		}

		public ServiceInstaller RegisterSingleton<T>(Func<T> factory) where T : class, new()
		{
			ServiceLocator.RegisterSingleton(typeof(T), factory);
			return this;
		}

		public ServiceInstaller RegisterSingleton(Type type, Func<object> factory)
		{
			ServiceLocator.RegisterSingleton(type, factory);
			return this;
		}

		public ServiceInstaller Register<T>(T service) where T : class
		{
			ServiceLocator.Register(service); //will throw exception here if there is a problem
			services.Add(typeof(T));
			return this;
		}

		public ServiceInstaller Register(Type type, object service)
		{
			ServiceLocator.Register(type, service); //will throw exception here if there is a problem
			services.Add(type);
			return this;
		}

		public ServiceInstaller TryRegister<T>(T service) where T : class
		{
			if (ServiceLocator.TryRegister(service))
				services.Add(typeof(T));
			return this;
		}

		public ServiceInstaller Build()
		{
			return this;
		}

		public void Dispose()
		{
			foreach (var type in services)
				ServiceLocator.Unregister(type);
			services.Clear();
		}
	}
}
