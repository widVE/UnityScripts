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
    public abstract class PathObject : MonoBehaviour, IObserver<PathCreator>
    {
		[SerializeField]
		PathCreator _path;
		public PathCreator Path
		{
			get => _path;
			protected set => _path = value;
		}

		PathObjectSequence _sequence;
		public PathObjectSequence Sequence
		{
			get => _sequence ?? (_sequence = PathObjectSequence.FindFromPath(Path));
			protected set => _sequence = value;
		}

		public abstract float Position { get; protected set; }

		public abstract void UpdatePosition(bool notify = true);

		public void SetPath(PathCreator path)
		{
			if(path == Path) return;

			if(Path) RemoveFromSequence(Path);
			Path = path;
			if(Path) AddToSequence(Path);
		}

		protected void AddToSequence(PathCreator path)
		{
			if(path)
			{
				PathObjectSequence sequence = PathObjectSequence.FindFromPath(path);
				sequence.Add(this);

				Sequence = sequence;
			}
		}

		protected void RemoveFromSequence(PathCreator path)
		{
			if(path)
			{
				PathObjectSequence sequence = PathObjectSequence.FindFromPath(path);
				sequence.Remove(this);

				Sequence = null;
			}
		}

		public abstract void OnNotify();

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(PathObject), true)]
		protected class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				PathObject pathObject = target as PathObject;

				serializedObject.Update();

				//draw path
				EditorGUI.BeginChangeCheck();

				SerializedProperty pathProperty = serializedObject.FindProperty(nameof(_path));

				EditorGUILayout.PropertyField(pathProperty);

				bool pathChanged = EditorGUI.EndChangeCheck();

				serializedObject.ApplyModifiedProperties();

				if(pathChanged)
				{
					PathCreator path = pathProperty.objectReferenceValue as PathCreator;

					foreach(PathObject po in targets)
					{
						po.RemoveFromSequence(po.Sequence.Path);
						po.AddToSequence(path);
					}
				}
			}
		}
#endif
	}
}