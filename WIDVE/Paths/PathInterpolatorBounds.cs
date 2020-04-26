using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
    [RequireComponent(typeof(PathPosition))]
    public class PathInterpolatorBounds : MonoBehaviour, IPathEvent
    {
        [SerializeField]
        PathInterpolator _parent;
        PathInterpolator Parent => _parent;

        PathPosition _position;
        public PathPosition Position => _position ? _position : (_position = GetComponent<PathPosition>());

        public static PathInterpolatorBounds GetClosestBounds(float position, PathInterpolatorBounds a, PathInterpolatorBounds b)
        {
            float d_a = Mathf.Abs(position - a.Position.Position);
            float d_b = Mathf.Abs(position - b.Position.Position);

            return d_a < d_b ? a : b;
        }

        public void Trigger(PathTrigger trigger)
        {
            if(!Parent) return;

            Parent.Activate(trigger.Position, this);
        }

        public void DrawGizmo(Color color)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            Gizmos.color = color;
            Gizmos.DrawCube(Vector3.zero, Vector3.one * .25f);
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(PathInterpolatorBounds), true)]
        class Editor : UnityEditor.Editor
        {
            [DrawGizmo(GizmoType.Active | GizmoType.Selected)]
            static void DrawGizmo(PathInterpolatorBounds pathInterpolatorBounds, GizmoType gizmoType)
            {
                pathInterpolatorBounds.DrawGizmo(Color.white);
            }
        }
#endif
    }
}