using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
	public class BoxColliderBounds : MonoBehaviour
	{
		[SerializeField]
		BoxCollider _collider;
		BoxCollider Collider => _collider;

		[SerializeField]
		Vector3 _padding = Vector3.zero;
		Vector3 Padding => _padding;

		public void MatchColliderToBounds()
		{
			Renderer boundsRenderer = GetComponentInParent<Renderer>();

			if(!boundsRenderer)
			{
				Debug.Log("Error! No object found to use as bounds.");
				return;
			}

			Bounds bounds = boundsRenderer.bounds;

			Collider.center = transform.InverseTransformPoint(bounds.center);
			Collider.size = (bounds.extents + Padding) * 2;

			gameObject.MarkSceneDirty();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(BoxColliderBounds))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				bool changed = EditorGUI.EndChangeCheck();

				if(GUILayout.Button("Match Bounds") || changed)
				{
					foreach(BoxColliderBounds bcb in targets)
					{
						bcb.MatchColliderToBounds();
					}
				}
			}
		}
#endif
	}
}