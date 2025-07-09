using UnityEngine;

namespace UnityServiceLocator
{
	[DefaultExecutionOrder(-100)]
	public class ServiceBehaviour : MonoBehaviour
	{
		[System.Serializable]
		public class ServiceClassType
		{
			[SerializeField] string assemblyQualifiedName;
			[SerializeField] bool asSingleton = false;

			public bool AsSingleton => asSingleton;

			public System.Type ClassType => System.Type.GetType(assemblyQualifiedName);
		}

		[Header("Settings")]
		[SerializeField] bool installOnAwake = true;
		[SerializeField] bool installOnStart = false;

		[Header("Classes")]
		[SerializeField] ServiceClassType[] classes = null;

		[Header("MonoBehaviours")]
		[SerializeField] MonoBehaviour[] monoBehaviours = null;

		[Header("ScriptableObjects")]
		[SerializeField] ScriptableObject[] scriptableObjects = null;

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

			foreach (var clazz in classes)
				if (clazz != null && clazz.ClassType != null)
					if (clazz.AsSingleton)
						serviceInstaller.RegisterSingleton(clazz.ClassType, () => System.Activator.CreateInstance(clazz.ClassType));
					else
						serviceInstaller.Register(clazz.ClassType, System.Activator.CreateInstance(clazz.ClassType));

			foreach (var monoBehaviour in monoBehaviours)
				if (monoBehaviour != null)
					serviceInstaller.Register(monoBehaviour.GetType(), monoBehaviour);

			foreach (var scriptableObject in scriptableObjects)
				if (scriptableObject != null)
					serviceInstaller.Register(scriptableObject.GetType(), scriptableObject);

			OnInstalled(serviceInstaller);

			serviceInstaller.Build();
		}

		virtual protected void OnPreInstall(ServiceInstaller installer)
		{
			//Override me
		}

		virtual protected void OnInstalled(ServiceInstaller installer)
		{
			//Override me
		}
	}
}
