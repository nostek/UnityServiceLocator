using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace UnityServiceLocator.Editor
{
	[CustomPropertyDrawer(typeof(ServiceBehaviour.ServiceClassType))]
	public class PropertyEditorServiceClassType : PropertyDrawer
	{
		static System.Type[] classes = null;

		readonly Dictionary<string, string> editing = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var prevLabelWidth = EditorGUIUtility.labelWidth;

			var assemblyQualifiedName = property.FindPropertyRelative("assemblyQualifiedName");
			var classType = !string.IsNullOrEmpty(assemblyQualifiedName.stringValue) ? System.Type.GetType(assemblyQualifiedName.stringValue) : null;

			if (editing.ContainsKey(property.propertyPath))
			{
				var editText = editing[property.propertyPath];

				//Textfield Selected class
				{
					var pos = position;
					pos.x = 140;
					pos.width -= 190;

					EditorGUIUtility.labelWidth = 50;
					editing[property.propertyPath] = EditorGUI.TextField(pos, editText);
				}

				//Button OK
				{
					var pos = position;
					pos.x = position.width - 45;
					pos.width = 60;

					EditorGUIUtility.labelWidth = prevLabelWidth;
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
					pos.x = 140;
					pos.width -= 190;

					EditorGUIUtility.labelWidth = 50;
					EditorGUI.LabelField(pos, classType != null ? $"{classType.Name} ({classType.Assembly.FullName[..classType.Assembly.FullName.IndexOf(',')]})" : "(None)");
				}

				//Button Select
				{
					var pos = position;
					pos.x = position.width - 45;
					pos.width = 60;

					EditorGUIUtility.labelWidth = prevLabelWidth;
					if (GUI.Button(pos, "Select"))
						editing[property.propertyPath] = classType?.FullName ?? string.Empty;
				}
			}

			//Toggle Singleton
			{
				var pos = position;
				pos.width = 64;
				EditorGUIUtility.labelWidth = 65;
				EditorGUI.PropertyField(pos, property.FindPropertyRelative("asSingleton"), new GUIContent("Singleton"));
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
