using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace WIDVE.Utilities
{
	public abstract class Spacer : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		List<Transform> _objects;
		public List<Transform> Objects
		{
			get => _objects ?? (_objects = new List<Transform>());
			set => _objects = value;
		}

		/// <summary>
		/// Returns the world position of the object at index.
		/// </summary>
		/// <param name="index">Object's index in the layout.</param>
		/// <returns>World position of object at index.</returns>
		public abstract Vector3 GetPosition(int index);

		public void LayoutObjects()
		{
			for(int i = 0; i < Objects.Count; i++)
			{
				Transform t = Objects[i];
				if(!t) continue;
				t.position = GetPosition(i);
			}
		}

		protected virtual string GetObjectName(int index)
		{
			if(index > Objects.Count - 1 || Objects[index] == null) return $"{index}: [Empty]";
			else return $"{index}: {Objects[index].name}";
		}

#if UNITY_EDITOR
		[DrawGizmo(GizmoType.Active)]
		static void DrawGizmos(Spacer spacer, GizmoType gizmoType)
		{
			Gizmos.matrix = Matrix4x4.identity;

			//draw starting point
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(spacer.GetPosition(0), .05f);

			//draw each middle position
			Gizmos.color = Color.blue;
			for(int i = 1; i < spacer.Objects.Count - 1; i++)
			{
				Gizmos.DrawSphere(spacer.GetPosition(i), .025f);
			}

			//draw ending point
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(spacer.GetPosition(spacer.Objects.Count - 1), .05f);
		}

		[CanEditMultipleObjects]
		[CustomEditor(typeof(Spacer), true)]
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
											label: new GUIContent((target as Spacer).GetObjectName(index)));
				};
			}

			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				EditorGUI.BeginChangeCheck();

				DrawDefaultInspector();
				Objects.DoLayoutList();

			
				serializedObject.ApplyModifiedProperties();

				if(EditorGUI.EndChangeCheck())
				{
					foreach(Spacer s in targets)
					{
						s.LayoutObjects();
					}
				}
			}
		}
#endif
	}
}