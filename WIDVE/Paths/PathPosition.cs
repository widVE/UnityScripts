using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Patterns;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
	[ExecuteAlways]
	public class PathPosition : PathObject, IInterpolatable
	{
		[SerializeField]
		[Range(0, 1)]
		float _position;
		public override float Position
		{
			get => _position;
			protected set => _position = value;
		}

		[SerializeField]
		[HideInInspector]
		bool[] _rotate = { false, false, false };
		public bool[] Rotate => _rotate;

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

		[SerializeField]
		bool _rotateOnStart;
		bool RotateOnStart => _rotateOnStart;

		public bool Enabled => enabled;

		public bool FunctionWhenDisabled => false;

		public float Distance => PositionToDistance(Position);

		float MaxDistance => Path ? Path.path.length : 0;

		public Vector3 WorldPosition => Path ? Path.path.GetPointAtTime(Position, EndInstruction) : Vector3.zero;

		public Vector3 Direction => Path ? Path.path.GetDirection(Position, EndInstruction) : Vector3.forward;

		public Quaternion Rotation => Path ? Path.path.GetRotation(Position, EndInstruction) : Quaternion.identity;

		public System.Action<float> OnPositionChanged;

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

		public void SetPosition(float position, bool saveWorldPosition = false, bool notify = true)
		{
			if(!Path) return;

			//validate position
			if(EndInstruction == EndOfPathInstruction.Loop) position = (position + 1) % 1;
			else position = Mathf.Clamp01(position);

			//set position
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
			SetRotation(Position, Rotate);

			//update the path object sequence when done
			PathObjectSequence sequence = PathObjectSequence.FindFromPath(Path);
			sequence.Sort();

			//notify that position has changed
			if(notify) OnPositionChanged?.Invoke(Position);
		}

		public void SetRotation(float position, bool[] rotationAxes = null)
		{
			if(!Path) return;

			//if all axes are locked, rotation will be reset to the default
			transform.localRotation = Quaternion.identity;

			//rotate on certain axes:
			if(rotationAxes == null) rotationAxes = new bool[] { false, false, false };
			if(rotationAxes[0] | rotationAxes[1] | rotationAxes[2])
			{
				Quaternion rotation = Path.path.GetRotation(position, EndInstruction);

				//lock any axes that shouldn't rotate
				Vector3 rotationAngles = rotation.eulerAngles;
				Vector3 i_rotationAngles = transform.localRotation.eulerAngles;
				for(int i = 0; i < 3; i++)
				{
					if(!rotationAxes[i]) rotationAngles[i] = i_rotationAngles[i];
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
			SetPosition(Path.path.GetClosestTimeOnPath(position), false);
		}

		public override void UpdatePosition(bool notify = true)
		{
			if(LockWorldPosition) SetWorldPosition(SavedWorldPosition);
			else SetPosition(Position, true, notify);
		}

		public override void OnNotify()
		{
			UpdatePosition();	
		}

		public void SetValue(float value)
		{
			SetPosition(value);
		}

		void OnEnable()
		{
			//add this object to the path's path object sequence
			AddToSequence(Path);
		}

		void Start()
		{
			if(Application.IsPlaying(this))
			{
				if(RotateOnStart) SetRotation(Position, Rotate);
			}
		}

		void OnDisable()
		{
			//remove this object from the path's path object sequence
			RemoveFromSequence(Path);
		}

#if UNITY_EDITOR
		public static void DrawRotationSettings(SerializedProperty rotationSettings)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label("Rotate:");

			string[] buttonNames = { "X", "Y", "Z" };
			for(int i = 0; i < rotationSettings.arraySize; i++)
			{
				SerializedProperty boolProperty = rotationSettings.GetArrayElementAtIndex(i);
				boolProperty.boolValue = GUILayout.Toggle(boolProperty.boolValue, buttonNames[i]);
			}

			GUILayout.EndHorizontal();
		}

		[CanEditMultipleObjects]
		[CustomEditor(typeof(PathPosition), true)]
		new class Editor : PathObject.Editor
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

				Undo.undoRedoPerformed += UpdatePosition;
			}

			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				serializedObject.Update();

				//draw settings
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_position)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_lockWorldPosition)));
				//EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_rotateOnStart))); //not working...

				//draw rotation settings
				DrawRotationSettings(serializedObject.FindProperty(nameof(_rotate)));

				bool changed = EditorGUI.EndChangeCheck();

				serializedObject.ApplyModifiedProperties();

				if(changed)
				{
					//set hide flags if they were not already set
					if(SHF == null) SetHideFlags();
					
					//update position
					foreach(PathPosition pp in targets)
					{
						UpdatePosition(pp);
					}
				}
			}

			void UpdatePosition()
			{
				UpdatePosition(target as PathPosition);
			}

			void UpdatePosition(PathPosition pathPosition)
			{
				pathPosition.SetPosition(pathPosition.Position, true);
				EditorUtility.SetDirty(pathPosition.transform);
			}

			void OnDisable()
			{
				//unlock transform
				if(SHF != null)
				{
					SHF.Undo();
					SHF = null;
				}

				Undo.undoRedoPerformed -= UpdatePosition;
			}
		}
#endif
	}
}