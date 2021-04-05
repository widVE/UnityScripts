using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    [ExecuteAlways]
    public class InterpolatorPulse : MonoBehaviour, IInterpolatable
    {
        [SerializeField]
        List<GameObject> Objects = new List<GameObject>();

        [SerializeField]
        AnimationCurve Pulse = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [SerializeField]
        [Range(.1f, 10)]
        float LoopTime = .5f;

        [SerializeField]
        [Range(0, 1)]
        float PulseStrength = .1f;

        float LastRawValue = 0;

        bool ValueSetThisFrame = false;

        public bool Enabled => enabled;

        public bool FunctionWhenDisabled => false;

        public float GetValue(float rawValue)
		{
            float time = Time.time;
            float pulseTime = (time % LoopTime) / LoopTime;
            float pulseValue = Pulse.Evaluate(pulseTime);
            return rawValue * (1 - (PulseStrength * (1 - pulseValue)));
		}

        public void SetValue(float rawValue)
		{
            float value = GetValue(rawValue);
            Debug.Log($"pulse value is {value} at time {Time.time}");

            for(int i = 0; i < Objects.Count; i++)
			{
                GameObject go = Objects[i];
                if(!go || !go.activeInHierarchy) continue;

                foreach(IInterpolatable ii in go.GetComponents<IInterpolatable>())
				{
                    if(!ii.IsActive()) continue;

                    ii.SetValue(value);
				}
			}

            LastRawValue = rawValue;
            ValueSetThisFrame = true;
		}

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
		{
            //call update to make the pulse redraw
            //https://forum.unity.com/threads/solved-how-to-force-update-in-edit-mode.561436/
            if(!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
        }
#endif

        void Update()
		{
            if(gameObject.ExistsInScene() && !ValueSetThisFrame) SetValue(LastRawValue);
		}
    }
}