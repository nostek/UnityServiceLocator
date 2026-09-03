using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace UnityServiceLocator.Editor
{
	[CustomPropertyDrawer(typeof(InstallServicesBehaviour.ServiceComponent))]
	public class PropertyEditorServiceComponent : PropertyDrawer
	{
		SelectClassDropDown dropdown = null;

		(SerializedProperty dirtyProperty, System.Type dirtyType)? dirty = null;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var prevLabelWidth = EditorGUIUtility.labelWidth;

			if (dirty.HasValue && dirty.Value.dirtyProperty.propertyPath == property.propertyPath)
			{
				var assemblyQualifiedName = property.FindPropertyRelative("assemblyQualifiedName");
				assemblyQualifiedName.stringValue = dirty.Value.dirtyType.AssemblyQualifiedName ?? string.Empty;

				dirty = null;
			}

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
					{
						ShowAdvancedDropDown(property.Copy());
					}
				}
			}

			EditorGUIUtility.labelWidth = prevLabelWidth;
			EditorGUI.EndProperty();
		}

		void ShowAdvancedDropDown(SerializedProperty property)
		{
			dropdown ??= new SelectClassDropDown(new AdvancedDropdownState());
			dropdown.ListenTo((selectedType) =>
			{
				dirty = (property, selectedType);
			});
			dropdown.Show(GUILayoutUtility.GetLastRect());
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		class SelectClassDropDown : AdvancedDropdown
		{
			System.Type[] classes;

			private event System.Action<System.Type> OnSelected;

			public SelectClassDropDown(AdvancedDropdownState state) : base(state)
			{
			}

			public void ListenTo(System.Action<System.Type> callback)
			{
				OnSelected = null;
				OnSelected += callback;
			}

			void BuildClasses()
			{
				if (classes != null)
					return;

				var types = TypeCache.GetTypesDerivedFrom(typeof(object));

				classes = types.Where(t => !t.IsAbstract && t.IsClass && !t.IsGenericType && t.IsPublic)
					.OrderBy(t => t.FullName)
					.ToArray();
			}

			protected override AdvancedDropdownItem BuildRoot()
			{
				BuildClasses();

				var category = new string[classes.Length];
				for (int i = 0; i < classes.Length; i++)
					category[i] = string.IsNullOrEmpty(classes[i].Namespace) ? null : classes[i].Namespace.Split('.')[0];

				var root = new AdvancedDropdownItem("Classes");

				var roots = new Dictionary<string, AdvancedDropdownItem>();

				for (int i = 0; i < classes.Length; i++)
				{
					if (string.IsNullOrEmpty(category[i]))
						continue;
					if (roots.ContainsKey(category[i]))
						continue;

					var item = new AdvancedDropdownItem(category[i]);
					root.AddChild(item);
					roots.Add(category[i], item);
				}

				root.AddSeparator();

				for (int i = 0; i < classes.Length; i++)
				{
					var iCopy = i;
					var parent = string.IsNullOrEmpty(category[i]) ? root : roots[category[i]];
					var item = new AdvancedDropdownItem(classes[i].FullName);
					parent.AddChild(item);
					item.id = iCopy;
				}

				return root;
			}

			protected override void ItemSelected(AdvancedDropdownItem item)
			{
				var ev = OnSelected;
				OnSelected = null;
				ev?.Invoke(classes[item.id]);
			}
		}
	}
}
