using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
    public class InterpolatorAnimator : MonoBehaviour
    {
        [SerializeField]
        Interpolator _interpolator;
        Interpolator Interpolator => _interpolator;

        [SerializeField]
        float _time = 1;
        float Time => _time;

        public void Play()
        {
            Interpolator.LerpTo1(Time);
        }

        public void PlayBackwards()
        {
            Interpolator.LerpTo0(Time);
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(InterpolatorAnimator))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                GUILayout.BeginHorizontal();

                if(GUILayout.Button("Play"))
                {
                    foreach(InterpolatorAnimator ia in targets)
                    {
                        ia.Play();
                    }
                }

                if(GUILayout.Button("Play Backwards"))
                {
                    foreach(InterpolatorAnimator ia in targets)
                    {
                        ia.PlayBackwards();
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
#endif
    }
}
