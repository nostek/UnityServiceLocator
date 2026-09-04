using UnityEngine;

namespace UnityServiceLocator
{
	[DefaultExecutionOrder(-100)]
	public class InstallServicesBehaviour : MonoBehaviour
	{
		public enum ServiceType
		{
			None = 0,
			MonoBehaviour,
			ScriptableObject,
			Class,
			SingletonClass
		}

		[System.Serializable]
		public class ServiceComponent
		{
			[SerializeField] ServiceType serviceType = ServiceType.None;
			public ServiceType ServiceType => serviceType;

			[SerializeField] MonoBehaviour monoBehaviour = null;
			public MonoBehaviour MonoBehaviour => monoBehaviour;

			[SerializeField] ScriptableObject scriptableObject = null;
			public ScriptableObject ScriptableObject => scriptableObject;

			[SerializeField] string assemblyQualifiedName;
			public System.Type ClassType => System.Type.GetType(assemblyQualifiedName);

			[SerializeField] string assemblyQualifiedNameForInterface;
			public System.Type InterfaceType => System.Type.GetType(assemblyQualifiedNameForInterface);

			[System.NonSerialized] public bool RegisteredSingleton = false;
		}

		[Header("Settings")]
		[SerializeField] bool installOnAwake = true;
		[SerializeField] bool installOnStart = false;

		[Header("Installation Order")]
		[SerializeField] ServiceComponent[] services = null;

		ServiceInstaller serviceInstaller = null;

		protected void Awake()
		{
			if (installOnAwake)
				Install();
		}

		protected void Start()
		{
			if (installOnStart)
				Install();
		}

		protected void OnDestroy()
		{
			serviceInstaller?.Dispose();
			serviceInstaller = null;
		}

		public void Install()
		{
			if (serviceInstaller != null)
				return;

			serviceInstaller = new ServiceInstaller();

			OnPreInstall(serviceInstaller);

			foreach (var service in services)
			{
				switch (service.ServiceType)
				{
					case ServiceType.Class:
						if (service.ClassType != null)
							if (service.InterfaceType != null)
								serviceInstaller.RegisterAs(service.ClassType, service.InterfaceType, System.Activator.CreateInstance(service.ClassType));
							else
								serviceInstaller.Register(service.ClassType, System.Activator.CreateInstance(service.ClassType));
						break;

					case ServiceType.SingletonClass:
						if (service.ClassType != null)
							if (service.InterfaceType != null)
								serviceInstaller.RegisterSingletonAs(service.ClassType, service.InterfaceType, () =>
								{
									service.RegisteredSingleton = true;
									return System.Activator.CreateInstance(service.ClassType);
								});
							else
								serviceInstaller.RegisterSingleton(service.ClassType, () =>
								{
									service.RegisteredSingleton = true;
									return System.Activator.CreateInstance(service.ClassType);
								});
						break;

					case ServiceType.MonoBehaviour:
						if (service.MonoBehaviour != null)
							if (service.InterfaceType != null)
								serviceInstaller.RegisterAs(service.MonoBehaviour.GetType(), service.InterfaceType, service.MonoBehaviour);
							else
								serviceInstaller.Register(service.MonoBehaviour.GetType(), service.MonoBehaviour);
						break;

					case ServiceType.ScriptableObject:
						if (service.ScriptableObject != null)
							if (service.InterfaceType != null)
								serviceInstaller.RegisterAs(service.ScriptableObject.GetType(), service.InterfaceType, service.ScriptableObject);
							else
								serviceInstaller.Register(service.ScriptableObject.GetType(), service.ScriptableObject);
						break;

					case ServiceType.None:
						break;
					default:
						throw new System.ArgumentOutOfRangeException();
				}
			}

			OnInstalled(serviceInstaller);

			serviceInstaller.Build();

			foreach (var service in services)
			{
				switch (service.ServiceType)
				{
					case ServiceType.Class:
						if (service.ClassType != null)
							Notify(service.ClassType);
						break;

					case ServiceType.SingletonClass:
						if (service.ClassType != null && service.RegisteredSingleton)
							Notify(service.ClassType);
						break;

					case ServiceType.MonoBehaviour:
						if (service.MonoBehaviour != null)
							Notify(service.MonoBehaviour.GetType());
						break;

					case ServiceType.ScriptableObject:
						if (service.ScriptableObject != null)
							Notify(service.ScriptableObject.GetType());
						break;

					case ServiceType.None:
						break;
					default:
						throw new System.ArgumentOutOfRangeException();
				}
			}
		}

		void Notify(System.Type type)
		{
			object @object = ServiceLocator.TryGet(type);
			if (@object != null && @object is IServiceInstalled @interface)
				@interface.OnServiceInstalled();
		}

		protected virtual void OnPreInstall(ServiceInstaller installer)
		{
			//Override me
		}

		protected virtual void OnInstalled(ServiceInstaller installer)
		{
			//Override me
		}
	}
}
