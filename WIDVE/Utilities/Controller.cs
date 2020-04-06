using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace WIDVE.Utilities
{
	public abstract class Controller : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Specifies which objects are affected by this component.")]
		GameObjectExtensions.SearchModes _mode = GameObjectExtensions.SearchModes.Self;
		protected GameObjectExtensions.SearchModes Mode => _mode;

		[SerializeField]
		[HideInInspector]
		List<GameObject> _objects;
		protected List<GameObject> Objects => _objects ?? (_objects = new List<GameObject>());

		protected abstract void UpdateControlledComponents();

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(Controller), true)]
		class Editor : UnityEditor.Editor
		{
			ReorderableList Objects;

			protected virtual void OnEnable()
			{
				Objects = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(_objects)),
											true, true, true, true);

				Objects.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Objects");
				};

				Objects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = Objects.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
											property: element,
											label: new GUIContent(index.ToString()));
				};
			}

			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				serializedObject.Update();

				DrawDefaultInspector();

				SerializedProperty mode = serializedObject.FindProperty(nameof(_mode));

				if(mode.enumValueIndex == (int)GameObjectExtensions.SearchModes.Custom)
				{
					Objects.DoLayoutList();
				}

				serializedObject.ApplyModifiedProperties();

				bool changed = EditorGUI.EndChangeCheck();

				if(changed)
				{
					foreach(Controller c in targets)
					{
						c.UpdateControlledComponents();
					}
				}
			}
		}
#endif
	}

    public abstract class Controller<T> : Controller where T : class
    {
		T[] _components;
		protected T[] Components
		{
			get => _components ?? (_components = GetControlledComponents());
			private set => _components = value;
		}

		protected override void UpdateControlledComponents()
		{
			Components = GetControlledComponents();
		}

		protected T[] GetControlledComponents()
		{
			return gameObject.GetComponentsInHierarchy<T>(Mode, Objects);
		}
	}
}