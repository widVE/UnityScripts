using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
    public class PathInterpolator : MonoBehaviour
    {
        [SerializeField]
        Interpolator _interpolator;
        public Interpolator Interpolator
        {
            get => _interpolator;
            set => _interpolator = value;
        }

        [SerializeField]
        PathInterpolatorBounds _start;
        PathInterpolatorBounds Start => _start;

        [SerializeField]
        PathInterpolatorBounds _end;
        PathInterpolatorBounds End => _end;

        PathPosition ActivePosition;

        public float GetValueFromPosition(float position)
        {
            if(!Start || !End) return 0;

            float startPosition = Start.Position.Position;
            float endPosition = End.Position.Position;
            float value;

            if(Mathf.Approximately(startPosition, endPosition))
            {
                //just return 0 if there is no room to interpolate
                value = 0;
            }
            else if(Mathf.Approximately(position, startPosition))
            {
                //at start
                value = 0;
            }
            else if(Mathf.Approximately(position, endPosition))
            {
                //at end
                value = 1;
            }
            else
            {
                //calculate the interpolation value based on the position's distance between start and end
                bool invert;
                float min, max;
                if(startPosition < endPosition)
                {
                    invert = false;
                    min = startPosition;
                    max = endPosition;
                }
                else
                {
                    invert = true;
                    min = endPosition;
                    max = startPosition;
                }

                position = Mathf.Clamp(position, min, max);

                value = (position - min) / (max - min);
                if(invert) value = 1 - value;
            }

            return value;
        }

        public void UpdateValue(float position)
        {
            if(!Interpolator) return;
            if(!Start || !End) return;

            float value = GetValueFromPosition(position);

            Interpolator.SetRawValue(value);
        }

        public void Activate(PathPosition position, PathInterpolatorBounds bounds)
        {
            if(!Start || !End)
            {
                //need both a start and end for the interpolator to function
                return;
            }

            if(!ActivePosition)
            {
                //a new trigger has entered
                position.OnPositionChanged += UpdateValue;
                ActivePosition = position;

                //set initial value
                UpdateValue(bounds.Position.Position);
            }
            else if(position == ActivePosition)
            {
                //notified again by the same trigger - this means it has left the bounds
                position.OnPositionChanged -= UpdateValue;
                ActivePosition = null;

                //set final value
                UpdateValue(bounds.Position.Position);
            }
            else
            {
                //notified by a different trigger
                //ignore it - for now, only worry about one trigger at a time
            }
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(PathInterpolator), true)]
        class Editor : UnityEditor.Editor
        {
            [DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy | GizmoType.NonSelected)]
            static void DrawBoundsGizmos(PathInterpolator pathInterpolator, GizmoType gizmoType)
            {
                if(pathInterpolator.Start)
                {
                    pathInterpolator.Start.DrawGizmo(Color.green);
                }

                if(pathInterpolator.End)
                {
                    pathInterpolator.End.DrawGizmo(Color.red);
                }
            }

            [DrawGizmo(GizmoType.Active)]
            static void DrawSelectionGizmos(PathInterpolator pathInterpolator, GizmoType gizmoType)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.blue;

                if(pathInterpolator.Start)
                {
                    Gizmos.DrawWireSphere(pathInterpolator.Start.Position.WorldPosition, .5f);
                }

                if(pathInterpolator.End)
                {
                    Gizmos.DrawWireSphere(pathInterpolator.End.Position.WorldPosition, .5f);
                }
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                PathInterpolator pathInterpolator = target as PathInterpolator;

                EditorGUILayout.HelpBox("Interpolator value will be 0 at Start, 1 at End.", MessageType.Info);

                GUI.enabled = false;

                //for debugging:
                if(Application.IsPlaying(this))
                {
                    if(pathInterpolator.ActivePosition)
                    {
                        EditorGUILayout.ObjectField("Active PathPosition:", pathInterpolator.ActivePosition, typeof(PathTrigger), true);

                        float triggerPosition = pathInterpolator.ActivePosition.Position;
                        EditorGUILayout.LabelField("Value:", $"{pathInterpolator.GetValueFromPosition(triggerPosition)} [at position {triggerPosition}]");
                        EditorGUILayout.LabelField("Start:", $"{pathInterpolator.Start.Position.Position}");
                        EditorGUILayout.LabelField("End:", $"{pathInterpolator.End.Position.Position}");
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No active PathPosition.");
                    }
                }

                GUI.enabled = true;
            }
        }
#endif
    }
}