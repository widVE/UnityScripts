using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Patterns;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
	public class PathPosition : MonoBehaviour, IObserver<PathCreator>
	{
		[SerializeField]
		PathCreator _path;
		public PathCreator Path
		{
			get => _path;
			set => _path = value;
		}

		[SerializeField]
		[Range(0, 1)]
		float _position;
		public float Position
		{
			get => _position;
			private set => _position = value;
		}

		[SerializeField]
		[HideInInspector]
		bool[] _rotate = { false, false, false };
		bool[] Rotate => _rotate;

		[SerializeField]
		bool _lockWorldPosition = false;
		bool LockWorldPosition => _lockWorldPosition;

		[SerializeField]
		[HideInInspector]
		Vector3 _savedWorldPosition;
		Vector3 SavedWorldPosition
		{
			get => _savedWorldPosition;
			set => _savedWorldPosition = value;
		}

		public float Distance => PositionToDistance(Position);

		float MaxDistance => Path ? Path.path.length : 0;

		public Vector3 WorldPosition => Path.path.GetPointAtTime(Position, EndInstruction);

		public Vector3 Direction => Path.path.GetDirection(Position, EndInstruction);

		EndOfPathInstruction EndInstruction
		{
			get
			{
				if(Path.path.isClosedLoop) return EndOfPathInstruction.Loop;
				else return EndOfPathInstruction.Stop;
			}
		}

		float DistanceToPosition(float distance)
		{
			return Mathf.Approximately(MaxDistance, 0) ? 0 : distance / MaxDistance;
		}

		float PositionToDistance(float position)
		{
			return MaxDistance * position;
		}

		public void SetPosition(float position, bool saveWorldPosition = false)
		{
			if(!Path) return;

			//validate position
			if(EndInstruction == EndOfPathInstruction.Loop) position = (position + 1) % 1;
			else position = Mathf.Clamp01(position);

			//set position property
			Position = position;

			//set world position based on path
			Vector3 worldPosition = Path.path.GetPointAtTime(position, EndInstruction);
			transform.position = worldPosition;

			//optional: save the set world position
			if(saveWorldPosition)
			{
				SavedWorldPosition = worldPosition;
			}

			//optional: set rotation based on path tangent at position
			if(Rotate[0] | Rotate[1] | Rotate[2])
			{
				Quaternion rotation = Path.path.GetRotation(position, EndInstruction);

				//lock any axes that shouldn't rotate
				Vector3 rotationAngles = rotation.eulerAngles;
				Vector3 i_rotationAngles = transform.rotation.eulerAngles;
				for(int i = 0; i < 3; i++)
				{
					if(!Rotate[i]) rotationAngles[i] = i_rotationAngles[i];
				}

				transform.rotation = Quaternion.Euler(rotationAngles);
			}
		}

		public void SetDistance(float distance, bool saveWorldPosition = false)
		{
			SetPosition(DistanceToPosition(distance), saveWorldPosition);
		}

		public void SetWorldPosition(Vector3 position)
		{
			if(!Path) return;

			//set Position to the path position closest to the provided world position
			Position = Path.path.GetClosestTimeOnPath(position);
			SetPosition(Position, false);
		}

		public void OnNotify()
		{
			if(LockWorldPosition) SetWorldPosition(SavedWorldPosition);
			else SetPosition(Position, true);
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(PathPosition))]
		class Editor : UnityEditor.Editor
		{
			ICommand SHF;

			void SetHideFlags()
			{
				PathPosition pathPosition = target as PathPosition;

				//don't hide transform if there is no path yet
				if(!pathPosition.Path) return;

				if(SHF != null) SHF.Undo();
				SHF = new UnityCommands.SetHideFlags(pathPosition.transform, HideFlags.NotEditable);
				SHF.Execute();
			}

			void OnEnable()
			{
				//lock transform
				PathPosition pathPosition = target as PathPosition;
				if(pathPosition.Path) SetHideFlags();
			}

			public override void OnInspectorGUI()
			{
				PathPosition pathPosition = target as PathPosition;

				EditorGUI.BeginChangeCheck();

				serializedObject.Update();

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_path)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_position)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_lockWorldPosition)));

				//draw rotation settings
				GUILayout.BeginHorizontal();
				
				SerializedProperty rotateProperty = serializedObject.FindProperty(nameof(_rotate));
				GUILayout.Label("Rotate:");

				string[] buttonNames = { "X", "Y", "Z" };
				for(int i = 0; i < rotateProperty.arraySize; i++)
				{
					SerializedProperty boolProperty = rotateProperty.GetArrayElementAtIndex(i);
					boolProperty.boolValue = GUILayout.Toggle(boolProperty.boolValue, buttonNames[i]);
				}

				GUILayout.EndHorizontal();

				serializedObject.ApplyModifiedProperties();

				if(EditorGUI.EndChangeCheck())
				{
					//set hide flags if they were not already set
					if(SHF == null) SetHideFlags();

					//update position on path
					pathPosition.SetPosition(pathPosition.Position, true);
				}
			}

			void OnDisable()
			{
				//unlock transform
				if(SHF != null)
				{
					SHF.Undo();
					SHF = null;
				}
			}
		}
#endif
	}
}