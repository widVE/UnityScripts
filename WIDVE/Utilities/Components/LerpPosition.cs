using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class LerpPosition : MonoBehaviour, IInterpolatable
    {
        [SerializeField]
        Vector3 _start;
        Vector3 Start => _start;

        [SerializeField]
        Vector3 _end;
        Vector3 End => _end;

        public bool Enabled => enabled;

        public bool FunctionWhenDisabled => false;

        public void SetPosition(float time)
		{
            transform.localPosition = Vector3.Lerp(Start, End, time);
		}

        public void SetValue(float value)
		{
            SetPosition(value);
		}
    }
}