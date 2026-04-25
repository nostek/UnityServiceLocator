using UnityEngine;

namespace UnityServiceLocator
{
	[DefaultExecutionOrder(-100)]
	public class ServiceBehaviour : MonoBehaviour
	{
		public enum ServiceType
		{
			None = 0,
			MonoBehaviour,
			ScriptableObject,
			Class
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
			[SerializeField] bool asSingleton = false;
			public System.Type ClassType => System.Type.GetType(assemblyQualifiedName);
			public bool AsSingleton => asSingleton;
		}

		[Header("Settings")]
		[SerializeField] bool installOnAwake = true;
		[SerializeField] bool installOnStart = false;

		[Header("Installation Order")]
		[SerializeField] ServiceComponent[] services = null;

		ServiceInstaller serviceInstaller = null;

		void Awake()
		{
			if (installOnAwake)
				Install();
		}

		void Start()
		{
			if (installOnStart)
				Install();
		}

		void OnDestroy()
		{
			if (serviceInstaller != null)
			{
				serviceInstaller.Dispose();
				serviceInstaller = null;
			}
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
							if (service.AsSingleton)
								serviceInstaller.RegisterSingleton(service.ClassType, () => System.Activator.CreateInstance(service.ClassType));
							else
								serviceInstaller.Register(service.ClassType, System.Activator.CreateInstance(service.ClassType));
						break;

					case ServiceType.MonoBehaviour:
						if (service.MonoBehaviour != null)
							serviceInstaller.Register(service.MonoBehaviour.GetType(), service.MonoBehaviour);
						break;

					case ServiceType.ScriptableObject:
						if (service.ScriptableObject != null)
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
