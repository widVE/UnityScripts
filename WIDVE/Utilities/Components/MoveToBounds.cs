using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MoveToBounds : MonoBehaviour
{
	public void Move()
	{
		Renderer r = GetComponentInParent<Renderer>();

		if(r)
		{
			Bounds bounds = r.bounds;
			Move(transform, bounds);
		}
		else
		{
			Debug.Log("No bounds found in parent objects!");
		}
	}

	public static void Move(Transform transform, Bounds bounds)
	{
		transform.position = bounds.center;
		transform.gameObject.MarkSceneDirty();
	}

#if UNITY_EDITOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MoveToBounds))]
	class Editor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			if(GUILayout.Button("Move"))
			{
				foreach(MoveToBounds mtb in targets)
				{
					mtb.Move();
				}
			}
		}
	}
#endif
}
