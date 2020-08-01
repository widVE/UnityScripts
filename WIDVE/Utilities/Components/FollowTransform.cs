using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
	[ExecuteAlways]
	public class FollowTransform : MonoBehaviour
	{
		[SerializeField]
		string _tag;
		public string Tag => _tag;

		[SerializeField]
		bool _followPosition = true;
		bool FollowPosition => _followPosition;

		[SerializeField]
		bool _followRotation = true;
		bool FollowRotation => _followRotation;

		[SerializeField]
		bool _followInEditor = false;
		bool FollowInEditor => _followInEditor;

		Transform _followMe;
		public Transform FollowMe => _followMe != null ? _followMe : (_followMe = GetFollowTarget());

		bool ShouldFollow
		{
			get
			{
				if(Application.IsPlaying(this)) return true;
				else if(FollowInEditor && gameObject.ExistsInScene()) return true;
				else return false;
			}
		}

		public Transform GetFollowTarget()
		{
			GameObject[] tagObjects = GameObject.FindGameObjectsWithTag(Tag);
			if(tagObjects.Length > 0)
			{
				return tagObjects[0].transform;
			}
			return null;
		}

		public void Follow()
		{
			if(!FollowMe) GetFollowTarget();
			if(!FollowMe) return;

			if(FollowPosition) transform.position = FollowMe.position;
			if(FollowRotation) transform.rotation = FollowMe.rotation;
		}

		void LateUpdate()
		{
			if(ShouldFollow) Follow();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(FollowTransform))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				SerializedProperty tag = serializedObject.FindProperty(nameof(_tag));
				tag.stringValue = EditorGUILayout.TagField(label: "Tag", tag: tag.stringValue);

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_followPosition)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_followRotation)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_followInEditor)));

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}
