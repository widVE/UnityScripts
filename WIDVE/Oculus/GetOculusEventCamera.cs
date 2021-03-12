using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Oculus
{
    public class GetOculusEventCamera : MonoBehaviour
    {
        [SerializeField]
        List<Canvas> _targetCanvas = new List<Canvas>();
        List<Canvas> TargetCanvas => _targetCanvas;

        [SerializeField]
        string _cameraTag;
        string CameraTag => _cameraTag;

		void Update()
		{
            GameObject oculusCamera_go = GameObject.FindGameObjectWithTag(CameraTag);
            if(!oculusCamera_go) return;

            Camera oculusCamera = oculusCamera_go.GetComponentInChildren<Camera>();
            if(!oculusCamera) return;

			foreach(Canvas c in TargetCanvas)
			{
                c.worldCamera = oculusCamera;
                c.gameObject.SetActive(false);
                c.gameObject.SetActive(true);
            }

            //done checking for oculus cameras and setting canvases
            enabled = false;
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(GetOculusEventCamera))]
        [CanEditMultipleObjects]
        class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_targetCanvas)), true);

                SerializedProperty cameraTag = serializedObject.FindProperty(nameof(_cameraTag));
                cameraTag.stringValue = EditorGUILayout.TagField(label: cameraTag.displayName, tag: cameraTag.stringValue);

                serializedObject.ApplyModifiedProperties();
			}
		}
#endif
    }
}