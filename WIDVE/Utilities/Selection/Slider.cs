using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class Slider : MonoBehaviour
    {
        [SerializeField]
        Transform _start;
        Transform Start => _start;

        [SerializeField]
        Transform _end;
        Transform End => _end;

        [SerializeField]
        Transform _valuePosition;
        Transform ValuePosition => _valuePosition;

        public float Value
		{
			get
			{
                //return a value between 0 and 1 based on the distance between start and end
                float d_start = Vector3.Distance(Start.position, ValuePosition.position);
                if(Mathf.Approximately(d_start, 0)) return 0;

                float d_end = Vector3.Distance(End.position, ValuePosition.position);
                if(Mathf.Approximately(d_end, 0)) return 1;

                return 0; //temp
			}
		}

        public void SetValue(float value)
		{

		}
    }
}