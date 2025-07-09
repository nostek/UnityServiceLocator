using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

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

#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(ServiceClassType))]
		public class PropertyEditorServiceClassType : PropertyDrawer
		{
			System.Type[] classes = null;
			string[] strings = null;

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				EditorGUI.BeginProperty(position, label, property);

				if (classes == null)
				{
					var types = TypeCache.GetTypesDerivedFrom(typeof(object));

					classes = types.Where(t => !t.IsAbstract && t.IsClass && !t.IsGenericType && t.IsPublic)
						.OrderBy(t => t.Name)
						.ToArray();

					strings = classes
						.Select(t => $"{t.Name[0]}/{t.Name}")
						.ToArray();
				}

				var assemblyQualifiedName = property.FindPropertyRelative("assemblyQualifiedName");
				var classType = !string.IsNullOrEmpty(assemblyQualifiedName.stringValue) ? System.Type.GetType(assemblyQualifiedName.stringValue) : null;
				var index = classType != null ? System.Array.IndexOf(classes, classType) : -1;

				var p1 = position;
				p1.height = EditorGUIUtility.singleLineHeight;
				var newIndex = EditorGUI.Popup(p1, "Class", index, strings);

				if (newIndex != index)
					assemblyQualifiedName.stringValue = classes[newIndex].AssemblyQualifiedName;

				var p2 = position;
				p2.y = EditorGUIUtility.singleLineHeight + 3f;
				p2.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(p2, property.FindPropertyRelative("asSingleton"));

				EditorGUI.EndProperty();
			}

			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return 3f + EditorGUIUtility.singleLineHeight * 2f;
			}
		}
#endif

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
