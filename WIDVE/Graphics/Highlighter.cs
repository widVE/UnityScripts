using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Graphics
{
	public class Highlighter : MonoBehaviour
	{
		[SerializeField]
		Interpolator _interpolator;
		Interpolator Interpolator => _interpolator;

		[SerializeField]
		[Range(0, 5)]
		float _activationTime = 2f;
		float ActivationTime => _activationTime;

		[SerializeField]
		[Range(0, 5)]
		float _deactivationTime = 1f;
		float DeactivationTime => _deactivationTime;

		public void Highlight(bool activate)
		{
			if(!Interpolator) return;

			if(activate) Interpolator.LerpTo1(ActivationTime);
			else Interpolator.LerpTo0(DeactivationTime);
		}
	}
}