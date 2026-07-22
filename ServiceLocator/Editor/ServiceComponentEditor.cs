using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace UnityServiceLocator.Editor
{
	[CustomPropertyDrawer(typeof(InstallServicesBehaviour.ServiceComponent))]
	public class PropertyEditorServiceComponent : PropertyDrawer
	{
		static System.Type[] classes = null;

		readonly Dictionary<string, string> editing = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var prevLabelWidth = EditorGUIUtility.labelWidth;

			var serviceType = property.FindPropertyRelative("serviceType");
			{
				var pos = position;
				pos.width = 135;

				EditorGUI.PropertyField(pos, serviceType, GUIContent.none);

				position.x += 135 + 3;
				position.width -= 135 + 3;
			}

			if (serviceType.intValue == (int)InstallServicesBehaviour.ServiceType.MonoBehaviour)
			{
				var field = property.FindPropertyRelative("monoBehaviour");
				EditorGUI.PropertyField(position, field, GUIContent.none);
			}

			if (serviceType.intValue == (int)InstallServicesBehaviour.ServiceType.ScriptableObject)
			{
				var field = property.FindPropertyRelative("scriptableObject");
				EditorGUI.PropertyField(position, field, GUIContent.none);
			}

			if (serviceType.intValue == (int)InstallServicesBehaviour.ServiceType.Class)
			{
				//Toggle Singleton
				{
					var pos = position;
					pos.width = 110;
					EditorGUIUtility.labelWidth = 60;
					EditorGUI.PropertyField(pos, property.FindPropertyRelative("asSingleton"), new GUIContent("Singleton"));
					EditorGUIUtility.labelWidth = prevLabelWidth;

					position.x += 110 + 3;
					position.width -= 110 + 3;
				}

				var assemblyQualifiedName = property.FindPropertyRelative("assemblyQualifiedName");
				var classType = !string.IsNullOrEmpty(assemblyQualifiedName.stringValue) ? System.Type.GetType(assemblyQualifiedName.stringValue) : null;

				if (editing.ContainsKey(property.propertyPath))
				{
					var editText = editing[property.propertyPath];

					//Textfield Selected class
					{
						var pos = position;
						pos.width -= 60 + 3;

						editing[property.propertyPath] = EditorGUI.TextField(pos, editText);

						position.x += pos.width + 3;
						position.width -= 3;
					}

					//Button OK
					{
						var pos = position;
						pos.width = 60;

						if (GUI.Button(pos, "OK"))
						{
							editing.Remove(property.propertyPath);

							BuildClasses();

							var found = !string.IsNullOrEmpty(editText) ? classes.FirstOrDefault(t => t.FullName.Contains(editText)) : null;
							assemblyQualifiedName.stringValue = found?.AssemblyQualifiedName ?? string.Empty;
						}
					}
				}
				else
				{
					//Label Selected class
					{
						var pos = position;
						pos.width -= 60 + 3;

						EditorGUI.LabelField(pos, classType != null ? $"{classType.Name} ({classType.Assembly.FullName[..classType.Assembly.FullName.IndexOf(',')]})" : "(None)");

						position.x += pos.width + 3;
						position.width -= 3;
					}

					//Button Select
					{
						var pos = position;
						pos.width = 60;

						if (GUI.Button(pos, "Select"))
							editing[property.propertyPath] = classType?.FullName ?? string.Empty;
					}
				}
			}

			EditorGUIUtility.labelWidth = prevLabelWidth;
			EditorGUI.EndProperty();
		}

		static void BuildClasses()
		{
			if (classes != null)
				return;

			var types = TypeCache.GetTypesDerivedFrom(typeof(object));

			classes = types.Where(t => !t.IsAbstract && t.IsClass && !t.IsGenericType && t.IsPublic)
				.OrderBy(t => t.Name)
				.ToArray();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}
	}
}
