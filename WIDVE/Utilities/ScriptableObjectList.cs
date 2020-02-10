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
	/// ScriptableObject that contains a reorderable list of other ScriptableObject.
	/// <para>Any derived classes must exist in their own script file, or Unity will not be able to serialize them.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ScriptableObjectList<T> : ScriptableObject, IEnumerable where T : ScriptableObject
	{
		// Since Unity can't serialize generics, a serialized List<T> must exist in the child class.
		// Access this list using the following properties:

		/// <summary>
		/// The name of the underlying serialized field used by the Objects property.
		/// <para>Needed when creating the reorderable list in the editor.</para>
		/// </summary>
		protected abstract string SerializedListName { get; }

		/// <summary>
		/// Access the underlying serailized list.
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

		public IEnumerator GetEnumerator()
		{
			return Objects.GetEnumerator();
		}

#if UNITY_EDITOR
		/// <summary>
		/// Custom editor that draws list contents as a reorderable list.
		/// <para>Extend this editor in any derived classes.</para>
		/// </summary>
		public abstract class Editor : UnityEditor.Editor
		{
			ScriptableObjectList<T> Target;
			ReorderableList List;

			/// <summary>
			/// Initialize the reorderable list.
			/// </summary>
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

			/// <summary>
			/// Draw the reorderable list.
			/// </summary>
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