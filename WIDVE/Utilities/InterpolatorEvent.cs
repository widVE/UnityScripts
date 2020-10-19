using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public class InterpolatorEvent : MonoBehaviour
	{
		[SerializeField]
		Interpolator _interpolator;
		Interpolator Interpolator => _interpolator;

		public void LerpTo0(float time)
		{
			Interpolator.LerpTo0(time);
		}

		public void LerpTo1(float time)
		{
			Interpolator.LerpTo1(time);
		}

		void OnValidate()
		{
			if(!Interpolator) _interpolator = GetComponent<Interpolator>();
		}
	}
}