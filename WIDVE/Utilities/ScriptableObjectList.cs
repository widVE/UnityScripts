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
	/// Lists should inherit from the generic ScriptableObjectList.
	/// </summary>
	public abstract class ScriptableObjectList : ScriptableObject
	{
		/// <summary>
		/// The name of the underlying serialized field used by the Objects property.
		/// <para>Needed when creating the reorderable list in the editor.</para>
		/// <para>The list should have the HideInInspector attribute, or it will be drawn twice.</para>
		/// </summary>
		protected abstract string SerializedListName { get; }

		/// <summary>
		/// List name to display in the inspector.
		/// </summary>
		protected abstract string GetListDisplayName();

		/// <summary>
		/// Element name to display in the inspector.
		/// </summary>
		protected abstract string GetElementDisplayName(int index);

#if UNITY_EDITOR
		/// <summary>
		/// Custom editor that draws list contents as a reorderable list.
		/// </summary>
		[CustomEditor(typeof(ScriptableObjectList), true)]
		protected class Editor : UnityEditor.Editor
		{
			ReorderableList List;

			/// <summary>
			/// Initializes the reorderable list.
			/// </summary>
			protected virtual void OnEnable()
			{
				ScriptableObjectList sol = target as ScriptableObjectList;

				List = new ReorderableList(serializedObject,
										   serializedObject.FindProperty(sol.SerializedListName),
										   true, true, true, true);

				List.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, sol.GetListDisplayName());
				};

				List.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = List.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
											property: element,
											label: new GUIContent(sol.GetElementDisplayName(index)));
				};
			}

			/// <summary>
			/// Draws the reorderable list.
			/// </summary>
			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				DrawDefaultInspector();
				List.DoLayoutList();

				serializedObject.ApplyModifiedProperties();
			}		
		}
#endif
	}

	/// <summary>
	/// ScriptableObject that contains a reorderable list of other ScriptableObjects.
	/// <para>Any derived classes must exist in their own script file, or Unity will not be able to serialize them.</para>
	/// </summary>
	public abstract class ScriptableObjectList<T> : ScriptableObjectList, IEnumerable where T : ScriptableObject
	{
		// Since Unity can't serialize generics, a serialized List<T> must exist in the child class.
		// Access this list using the following properties:

		/// <summary>
		/// Access the underlying serialized list.
		/// </summary>
		protected abstract List<T> Objects { get; }

		/// <summary>
		/// Number of elements in the list.
		/// </summary>
		public int Count => Objects.Count;

		public T this[int index]
		{
			get => Objects[index];
			set => Objects[index] = value;
		}

		public void Add(T t)
		{
			Objects.Add(t);
		}

		public bool Remove(T t)
		{
			return Objects.Remove(t);
		}

		public void RemoveAt(int index)
		{
			Objects.RemoveAt(index);
		}

		public IEnumerator GetEnumerator()
		{
			return Objects.GetEnumerator();
		}

		protected override string GetListDisplayName()
		{
			return $"{typeof(T).Name} List";
		}

		protected override string GetElementDisplayName(int index)
		{
			return $"{typeof(T).Name} {index}";
		}
	}
}