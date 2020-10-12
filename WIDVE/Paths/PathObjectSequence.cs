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
	public class PathObjectSequence : MonoBehaviour, IObserver<PathCreator>, IEnumerable
	{
		[SerializeField]
		PathCreator _path;
		public PathCreator Path
		{
			get => _path;
			private set => _path = value;
		}

		[SerializeField]
		List<PathObject> _objects;
		public List<PathObject> Objects
		{
			get =>	_objects ?? (_objects = new List<PathObject>());
			private set => _objects = value;
		}

		public PathObject this[int index] => Objects[index];

		public int Count => Objects.Count;

		public IEnumerator GetEnumerator()
		{
			for(int i = 0; i < Objects.Count; i++)
			{
				yield return Objects[i];
			}
		}

		public void Add(PathObject pathObject)
		{
			if(!Objects.Contains(pathObject))
			{
				Objects.Add(pathObject);
				Sort();
			}
		}

		public bool Remove(PathObject pathObject)
		{
			return Objects.Remove(pathObject);
		}

		public void UpdatePositions()
		{
			//update the position of every object in the sequence
			for(int i = 0; i < Objects.Count; i++)
			{
				PathObject po = Objects[i];
				if(!po) continue;

				po.UpdatePosition(false);
			}
		}

		public void Sort()
		{
			//remove any null path objects
			for(int i = Objects.Count - 1; i >= 0; i--)
			{
				if(!Objects[i]) Objects.RemoveAt(i);
			}

			//sort path events by their position on the path
			Objects.Sort((x, y) => x.Position.CompareTo(y.Position));
		}

		/// <summary>
		/// Returns the index of the last object on the path preceeding the given position.
		/// <para>Returns -1 if the given position is before all other objects in the list.</para>
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public int GetPrevIndex(float position)
		{
			int index = -1;

			for(int i = 0; i < Objects.Count; i++)
			{
				float currentPosition = Objects[i].Position;
				if(currentPosition < position)
				{
					index = i;
					continue;
				}

				break;
			}

			return index;
		}

		/// <summary>
		/// Returns the index of the first object on the path exceeding the given position.
		/// <para>Returns the Count of the list if the given position is past the last object's position.</para>
		/// </summary>
		public int GetNextIndex(float position)
		{
			int index = Objects.Count;

			for(int i = 0; i < Objects.Count; i++)
			{
				float currentPosition = Objects[i].Position;
				if(currentPosition > position)
				{
					index = i;
					break;
				}
			}

			return index;
		}

		/// <summary>
		/// Returns all PathObjects between startPosition (inclusive) and endPosition (exclusive).
		/// <para>Objects are ordered from least to greatest position on the path.</para>
		/// </summary>
		public List<PathObject> GetObjects(float startPosition, float endPosition)
		{
			return GetObjects(startPosition, endPosition, true, false);
		}

		/// <summary>
		/// Returns all PathObjects between startPosition (inclusive) and endPosition (inclusive).
		/// <para>Objects are ordered from least to greatest position on the path.</para>
		/// </summary>
		public List<PathObject> GetObjectsInclusive(float startPosition, float endPosition)
		{
			return GetObjects(startPosition, endPosition, true, true);
		}

		/// <summary>
		/// Returns all PathObjects between startPosition (exclusive) and endPosition (exclusive).
		/// <para>Objects are ordered from least to greatest position on the path.</para>
		/// </summary>
		public List<PathObject> GetObjectsExclusive(float startPosition, float endPosition)
		{
			return GetObjects(startPosition, endPosition, false, false);
		}

		List<PathObject> GetObjects(float startPosition, float endPosition, bool inclusiveStart, bool inclusiveEnd)
		{
			List<PathObject> objects = new List<PathObject>();

			float inclusivePosition1 = startPosition;
			float inclusivePosition2 = endPosition;

			//start should be before end - swap them otherwise
			if(startPosition > endPosition)
			{
				float _sp = startPosition;
				startPosition = endPosition;
				endPosition = _sp;
			}

			//get all objects between the two positions
			for(int i = 0; i < Objects.Count; i++)
			{
				PathObject pathObject = Objects[i];

				float position = pathObject.Position;

				if((position > startPosition && position < endPosition) ||
				   (inclusiveStart && Mathf.Approximately(position, inclusivePosition1)) ||
				   (inclusiveEnd && Mathf.Approximately(position, inclusivePosition2)))
				{
					objects.Add(pathObject);
				}
			}

			return objects;
		}

		/// <summary>
		/// Returns the PathObjectSequence associated with the given PathCreator.
		/// <para>Creates a new PathObjectSequence if one does not exist.</para>
		/// </summary>
		public static PathObjectSequence FindFromPath(PathCreator path)
		{
			if(!path) return null;

			PathObjectSequence sequence = path.GetComponent<PathObjectSequence>();

			if(!sequence)
			{
				sequence = path.gameObject.AddComponent<PathObjectSequence>();
				sequence.Path = path;
			}

			return sequence;
		}

		public void OnNotify()
		{
			UpdatePositions();

			Sort();
		}

		void OnEnable()
		{
			if(!Path) Path = GetComponent<PathCreator>();

			Sort();
		}

		void OnDrawGizmosSelected()
		{
			if(!enabled) return;

			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.gray;

			float scale = .25f * transform.lossyScale.x;

			foreach(PathObject po in Objects)
			{
				Gizmos.DrawWireSphere(Path.path.GetPointAtTime(po.Position), scale);
			}
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(PathObjectSequence))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_path)));

				EditorGUILayout.LabelField("Objects:", EditorStyles.boldLabel);

				GUI.enabled = false;

				EditorGUI.indentLevel++;

				SerializedProperty objects = serializedObject.FindProperty(nameof(_objects));
				List<PathObject> objectsList = objects.MakeList<PathObject>();
				for(int i = 0; i < objectsList.Count; i++)
				{
					PathObject pathObject = objectsList[i];
					EditorGUILayout.ObjectField($"{i} [{pathObject.Position}]", pathObject, typeof(PathObject), allowSceneObjects: true);
				}

				EditorGUI.indentLevel--;

				GUI.enabled = true;

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}