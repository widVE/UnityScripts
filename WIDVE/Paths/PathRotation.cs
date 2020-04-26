using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
    public class PathRotation : MonoBehaviour
    {
        [SerializeField]
        PathPosition _pathPosition;
        PathPosition PathPosition => _pathPosition;

        [SerializeField]
        bool _rotateInEditMode = false;
        bool RotateInEditMode => _rotateInEditMode;

        [SerializeField]
        bool _rotateOnStart = false;
        bool RotateOnStart => _rotateOnStart;

        [SerializeField]
        [HideInInspector]
        bool[] _rotate = { false, false, false };
        public bool[] Rotate => _rotate;

        bool ShouldRotate => Application.IsPlaying(this) || RotateInEditMode;

        void SetRotation(float position)
        {
            if(!PathPosition) return;

            PathPosition.SetRotation(position, Rotate);           
        }

        void EnableRotation(PathPosition pathPosition)
        {
            if(!ShouldRotate) return;

            if(pathPosition) pathPosition.OnPositionChanged += SetRotation;
        }

        void DisableRotation(PathPosition pathPosition)
        {
            if(!ShouldRotate) return;

            if(pathPosition) pathPosition.OnPositionChanged -= SetRotation;
        }

        void OnEnable()
        {
            EnableRotation(PathPosition);
        }

        void Start()
        {
            if(RotateOnStart)
            {
                if(ShouldRotate)
                {
                    if(PathPosition) SetRotation(PathPosition.Position);
                }
            }
        }

        void OnDisable()
        {
            DisableRotation(PathPosition);
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(PathRotation))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                //reset subscription in case path position gets changed
                foreach(PathRotation pr in targets)
                {
                    pr.DisableRotation(pr.PathPosition);
                }

                serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_pathPosition)));

                EditorGUILayout.HelpBox("PathPosition rotation settings will be overriden at the following times:", MessageType.Info);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_rotateInEditMode)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_rotateOnStart)));

                PathPosition.DrawRotationSettings(serializedObject.FindProperty(nameof(_rotate)));

                bool changed = EditorGUI.EndChangeCheck();

                serializedObject.ApplyModifiedProperties();

                if(changed)
                {
                    foreach(PathRotation pr in targets)
                    {
                        if(pr.ShouldRotate && pr.PathPosition)
                        {
                            pr.SetRotation(pr.PathPosition.Position);
                        }
                    }
                }

                foreach(PathRotation pr in targets)
                {
                    pr.EnableRotation(pr.PathPosition);
                }
            }
        }
#endif
    }
}