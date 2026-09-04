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

		struct DirtyType
		{
			public SerializedProperty Property;
			public System.Type Type;
		}

		DirtyType? dirty = null;
		DirtyType? dirtyInterface = null;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var prevLabelWidth = EditorGUIUtility.labelWidth;

			if (dirty.HasValue && dirty.Value.Property.propertyPath == property.propertyPath)
			{
				var assemblyQualifiedName = property.FindPropertyRelative("assemblyQualifiedName");
				assemblyQualifiedName.stringValue = dirty.Value.Type.AssemblyQualifiedName ?? string.Empty;

				dirty = null;
			}

			if (dirtyInterface.HasValue && dirtyInterface.Value.Property.propertyPath == property.propertyPath)
			{
				var assemblyQualifiedNameForInterface = property.FindPropertyRelative("assemblyQualifiedNameForInterface");
				assemblyQualifiedNameForInterface.stringValue = dirtyInterface.Value.Type.AssemblyQualifiedName ?? string.Empty;

				dirtyInterface = null;
			}

			System.Type interfacesForType = null;

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
				var pos = position;
				pos.width = pos.width / 2 - 2;

				var field = property.FindPropertyRelative("monoBehaviour");
				EditorGUI.PropertyField(pos, field, GUIContent.none);

				interfacesForType = field.objectReferenceValue != null ? field.objectReferenceValue.GetType() : null;

				position.x += pos.width + 2;
				position.width -= pos.width + 2;
			}

			if (serviceType.intValue == (int)InstallServicesBehaviour.ServiceType.ScriptableObject)
			{
				var pos = position;
				pos.width = pos.width / 2 - 2;

				var field = property.FindPropertyRelative("scriptableObject");
				EditorGUI.PropertyField(pos, field, GUIContent.none);

				interfacesForType = field.objectReferenceValue != null ? field.objectReferenceValue.GetType() : null;

				position.x += pos.width + 2;
				position.width -= pos.width + 2;
			}

			if (serviceType.intValue == (int)InstallServicesBehaviour.ServiceType.Class || serviceType.intValue == (int)InstallServicesBehaviour.ServiceType.SingletonClass)
			{
				var pos = position;
				pos.width = pos.width / 2 - 2;

				var assemblyQualifiedName = property.FindPropertyRelative("assemblyQualifiedName");
				var classType = !string.IsNullOrEmpty(assemblyQualifiedName.stringValue) ? System.Type.GetType(assemblyQualifiedName.stringValue) : null;

				interfacesForType = classType;

				//Label
				{
					var posi = pos;
					posi.width -= 19;

					EditorGUI.LabelField(posi, classType != null ? $"{classType.Name} ({classType.Assembly.FullName[..classType.Assembly.FullName.IndexOf(',')]})" : "None (Class)", EditorStyles.objectField);

					pos.x += posi.width;
				}

				//Button Select
				{
					var posi = pos;
					posi.width = 19;

					if (GUI.Button(posi, EditorGUIUtility.IconContent("d_pick"), EditorStyles.objectFieldThumb))
					{
						ShowAdvancedDropDown(property.Copy());
					}
				}

				position.x += pos.width + 2;
				position.width -= pos.width + 2;
			}

			//Interface picker
			{
				var assemblyQualifiedNameForInterface = property.FindPropertyRelative("assemblyQualifiedNameForInterface");
				var interfaceType = !string.IsNullOrEmpty(assemblyQualifiedNameForInterface.stringValue) ? System.Type.GetType(assemblyQualifiedNameForInterface.stringValue) : null;

				var interfaces = interfacesForType?.GetInterfaces();
				if ((interfaces == null || interfaces.Length == 0) && !string.IsNullOrEmpty(assemblyQualifiedNameForInterface.stringValue))
				{
					assemblyQualifiedNameForInterface.stringValue = string.Empty;
					interfaceType = null;
				}

				//Label
				{
					var pos = position;
					pos.width -= 19;

					EditorGUI.LabelField(pos, interfaceType != null ? $"{interfaceType.Name} ({interfaceType.Assembly.FullName[..interfaceType.Assembly.FullName.IndexOf(',')]})" : "None (Interface)", EditorStyles.objectField);

					position.x += pos.width;
				}

				//Button Select
				if (interfaces != null && interfaces.Length > 0)
				{
					var pos = position;
					pos.width = 19;

					if (GUI.Button(pos, EditorGUIUtility.IconContent("d_pick"), EditorStyles.objectFieldThumb))
					{
						GenericMenu menu = new();
						foreach (var i in interfaces)
							menu.AddItem(new GUIContent(i.Name), false, (t) => { dirtyInterface = (DirtyType)t; }, new DirtyType() { Property = property.Copy(), Type = i });
						menu.ShowAsContext();
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
				dirty = new DirtyType { Property = property, Type = selectedType };
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
