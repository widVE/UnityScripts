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
		public enum ObjectFindModes { Children, Custom }

		[SerializeField]
		[HideInInspector]
		[Tooltip("Specifies which objects will be part of the automatic layout.")]
		ObjectFindModes _objectsMode = ObjectFindModes.Children;
		protected ObjectFindModes ObjectsMode => _objectsMode;

		[SerializeField]
		[HideInInspector]
		List<Transform> _objects;
		public List<Transform> Objects
		{
			get => _objects ?? (_objects = new List<Transform>());
			set => _objects = value;
		}

		/// <summary>
		/// Returns true if this Spacer should rotate objects, in addition to positioning them.
		/// </summary>
		protected abstract bool RotateObjects { get; }

		/// <summary>
		/// Returns the total number of objects being laid out.
		/// </summary>
		protected int NumObjects
		{
			get
			{
				switch(ObjectsMode)
				{
					default:
					case ObjectFindModes.Children:
						return transform.childCount;

					case ObjectFindModes.Custom:
						return Objects.Count;
				}
			}
		}

		/// <summary>
		/// Returns the world position of the object at index.
		/// </summary>
		/// <param name="index">Object's index in the layout.</param>
		/// <returns>World position of object at index.</returns>
		public abstract Vector3 GetPosition(int index);

		/// <summary>
		/// Returns the rotation of the object at index.
		/// </summary>
		/// <param name="index">Object's index in the layout.</param>
		/// <returns>Rotation of object at index.</returns>
		public virtual Quaternion GetRotation(int index) { return Quaternion.identity; }

		protected static float GetTime(int index, int numObjects)
		{
			return (numObjects <= 1) ? .5f : (index / (float)(numObjects - 1));
		}

		protected List<Transform> GetObjects()
		{
			List<Transform> objects;
			switch(ObjectsMode)
			{
				default:
				case ObjectFindModes.Children:
					//use objects from direct children
					objects = new List<Transform>(transform.childCount);
					for(int i = 0; i < transform.childCount; i++)
					{
						objects.Add(transform.GetChild(i));
					}
					return objects;

				case ObjectFindModes.Custom:
					//use objects specified in inspector
					return Objects;
			}
		}

		public virtual void LayoutObjects()
		{
			//get objects
			List<Transform> objects = GetObjects();

			//layout objects
			for(int i = 0; i < objects.Count; i++)
			{
				Transform t = objects[i];
				if(!t) continue;
				t.position = GetPosition(i);

				//optional: rotate objects as well
				if(RotateObjects)
				{
					t.rotation = GetRotation(i);
				}
			}
		}

		public virtual void LoadObjectsFromChildren()
		{
			Objects.Clear();
			for(int i = 0; i < transform.childCount; i++)
			{
				Objects.Add(transform.GetChild(i));
			}
		}

		protected virtual string GetObjectName(int index)
		{
			if(index > Objects.Count - 1 || Objects[index] == null) return $"{index}: [Empty]";
			else return $"{index}: {Objects[index].name}";
		}

		protected virtual void DrawGizmos()
		{
			Gizmos.matrix = Matrix4x4.identity;

			//draw starting point
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(GetPosition(0), .05f);

			//draw each middle position
			Gizmos.color = Color.blue;
			List<Transform> objects = GetObjects();
			for(int i = 1; i < NumObjects - 1; i++)
			{
				Gizmos.DrawSphere(GetPosition(i), .025f);
			}

			//draw ending point
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(GetPosition(NumObjects - 1), .05f);
		}

#if UNITY_EDITOR
		[DrawGizmo(GizmoType.Active)]
		static void DrawGizmos(Spacer spacer, GizmoType gizmoType)
		{
			spacer.DrawGizmos();
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

				//optionally show list of objects based on mode
				SerializedProperty objectsMode = serializedObject.FindProperty(nameof(_objectsMode));
				EditorGUILayout.PropertyField(objectsMode);
				if(objectsMode.enumValueIndex == (int)ObjectFindModes.Custom)
				{
					Objects.DoLayoutList();

					//show buttons
					if((target as Spacer).Objects.Count == 0)
					{
						if(GUILayout.Button("Load Objects From Children"))
						{
							Undo.RecordObjects(targets, "Loaded Objects From Children");
							foreach(Spacer s in targets)
							{
								s.LoadObjectsFromChildren();
							}
						}
					}

					if(GUILayout.Button("Clear Objects"))
					{
						Undo.RecordObjects(targets, "Cleared Objects");
						foreach(Spacer s in targets)
						{
							s.Objects.Clear();
						}
					}
				}

				if(GUILayout.Button("Layout")) { /*pressing this will trigger a layout*/ }

				bool changed = EditorGUI.EndChangeCheck();
			
				serializedObject.ApplyModifiedProperties();

				if(changed)
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