using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Oculus
{
    public class GetOculusPointer : MonoBehaviour
    {
        [SerializeField]
        List<OVRRaycaster> _targetRaycaster = new List<OVRRaycaster>();
        List<OVRRaycaster> TargetRaycaster => _targetRaycaster;

        [SerializeField]
        string _pointerTag;
        string PointerTag => _pointerTag;

        void Update()
        {
            GameObject oculusPointer = GameObject.FindGameObjectWithTag(PointerTag);
            if(!oculusPointer) return;

            foreach(OVRRaycaster r in TargetRaycaster)
            {
                r.pointer = oculusPointer;
            }

            enabled = false;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(GetOculusPointer))]
        [CanEditMultipleObjects]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_targetRaycaster)), true);

                SerializedProperty pointerTag = serializedObject.FindProperty(nameof(_pointerTag));
                pointerTag.stringValue = EditorGUILayout.TagField(label: pointerTag.displayName, tag: pointerTag.stringValue);

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}