using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace WIDVE.Utilities
{
	/// <summary>
	/// Extend this class to draw a list of ScriptableObjects as a reorderable list.
	/// </summary>
	public abstract class ScriptableObjectList<T> : ScriptableObject where T : ScriptableObject
	{
		/// <summary>
		/// The name of the underlying serialized field used by the Objects property.
		/// </summary>
		protected abstract string SerializedListName { get; }

		/// <summary>
		/// Returns a serialized, non-generic list of objects of type T.
		/// </summary>
		protected abstract List<T> Objects { get; }

		/// <summary>
		/// Number of elements in the list.
		/// </summary>
		public int Count => Objects.Count;

		/// <summary>
		/// Access list elements by index.
		/// </summary>
		public T this[int index]
		{
			get => Objects[index];
			set => Objects[index] = value;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Custom editor that draws list contents as a reorderable list.
		/// </summary>
		public class Editor : UnityEditor.Editor
		{
			ScriptableObjectList<T> Target;
			ReorderableList List;

			protected virtual void OnEnable()
			{
				Target = target as ScriptableObjectList<T>;
				List = new ReorderableList(serializedObject,
										   serializedObject.FindProperty(Target.SerializedListName),
										   true, true, true, true);

				List.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "List");
				};

				List.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = List.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
											property: element,
											label: new GUIContent($"Element {index}"));
				};
			}

			public override void OnInspectorGUI()
			{
				serializedObject.Update();
				List.DoLayoutList();
				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}