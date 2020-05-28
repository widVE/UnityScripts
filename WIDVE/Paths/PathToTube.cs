using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Patterns;
using WIDVE.Utilities;
using PathCreation;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
	public class PathToTube : MonoBehaviour, IObserver<PathCreator>
	{
		[SerializeField]
		PathCreator _path;
		public PathCreator Path => _path;

		[SerializeField]
		TubeRenderer _tube;
		public TubeRenderer Tube => _tube;

		public void MakeTube()
		{
			MakeTube(Path, Tube);
		}

		public static void MakeTube(PathCreator path, TubeRenderer tube)
		{
			if(!path) return;
			if(!tube) return;

			//set up points array for the tube
			int totalPoints = path.path.localPoints.Length;
			Vector3[] tubePoints = new Vector3[totalPoints];

			//transform points from path space to tube space
			for(int i = 0; i < totalPoints; i++)
			{
				Vector3 pathLocalPoint = path.path.localPoints[i];
				Vector3 worldPoint = path.transform.TransformPoint(pathLocalPoint);
				Vector3 tubeLocalPoint = tube.transform.InverseTransformPoint(worldPoint);
				tubePoints[i] = tubeLocalPoint;
			}

			//assign points to the tube
			try
			{
				tube.points = tubePoints;
				tube.ForceUpdate();
				tube.SetDirty();
			}
			catch(UnassignedReferenceException) { }
		}

		public void OnNotify()
		{
			MakeTube();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(PathToTube))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				bool changed = EditorGUI.EndChangeCheck();

				if(GUILayout.Button("Create Tube") || changed)
				{
					foreach(PathToTube ptt in targets)
					{
						ptt.MakeTube();
					}
				}
			}
		}
#endif
	}
}