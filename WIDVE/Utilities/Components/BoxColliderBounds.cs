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

		public void MatchColliderToBounds()
		{
			Renderer boundsObject = GetComponentInParent<Renderer>();
			if(!boundsObject)
			{
				Debug.Log("Error! No object found to use as bounds.");
				return;
			}

			Bounds bounds = boundsObject.bounds;

			Collider.center = bounds.center;
			Collider.size = bounds.extents * 2;
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(BoxColliderBounds))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				if(GUILayout.Button("Match Bounds"))
				{
					foreach(Object t in targets)
					{
						BoxColliderBounds bcb = t as BoxColliderBounds;
						bcb.MatchColliderToBounds();
					}
				}
			}
		}
#endif
	}
}