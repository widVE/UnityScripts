using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
    [ExecuteAlways]
    public class FollowPathPosition : MonoBehaviour
    {
		[SerializeField]
		string _tag;
		public string Tag => _tag;

		PathPosition _follower;
		PathPosition Follower => _follower != null ? _follower : (_follower = GetComponent<PathPosition>());

		PathPosition _followMe;
		PathPosition FollowMe => _followMe != null ? _followMe : (_followMe = GetFollowTarget());

		public PathPosition GetFollowTarget()
		{
			if(string.IsNullOrEmpty(Tag)) return null;

			GameObject[] tagObjects = GameObject.FindGameObjectsWithTag(Tag);

			PathPosition followMe = null;
			foreach(GameObject go in tagObjects)
			{
				followMe = go.GetComponentInChildren<PathPosition>();
				if(followMe) break;
			}

			return followMe;
		}

		public void Follow()
		{
			if(!FollowMe) GetFollowTarget();
			if(!FollowMe) return;
			if(!Follower) return;

			Follower.SetPosition(FollowMe.Position);
		}

		void LateUpdate()
		{
			Follow();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(FollowPathPosition))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				SerializedProperty tag = serializedObject.FindProperty(nameof(_tag));
				tag.stringValue = EditorGUILayout.TagField(label: "Tag", tag: tag.stringValue);

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}