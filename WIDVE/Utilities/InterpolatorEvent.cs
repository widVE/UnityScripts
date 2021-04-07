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

		[SerializeField]
		[Tooltip("Attack/Hold/Release")]
		Vector3 _envelope = Vector3.one;
		Vector3 Envelope => _envelope;

		bool Active = false;

		public void LerpTo0(float time)
		{
			Interpolator.LerpTo0(time);
			Active = false;
		}

		public void LerpTo1(float time)
		{
			Interpolator.LerpTo1(time);
			Active = true;
		}

		public void Toggle(float time)
		{
			if(Active) LerpTo0(time);
			else LerpTo1(time);
		}

		public void PlayEnvelope()
		{
			StopAllCoroutines();
			StartCoroutine(PlayEnvelope(Envelope.x, Envelope.y, Envelope.z));
		}

		IEnumerator PlayEnvelope(float attack, float hold, float release)
		{
			Interpolator.LerpTo1(attack);
			yield return new WaitForSeconds(attack + hold);
			Interpolator.LerpTo0(release);
		}

		void OnValidate()
		{
			if(!Interpolator) _interpolator = GetComponent<Interpolator>();
			if(_envelope.x < 0) _envelope.x = 0;
			if(_envelope.y < 0) _envelope.y = 0;
			if(_envelope.z < 0) _envelope.z = 0;
		}

		void Start()
		{
			Active = Mathf.Approximately(Interpolator.Value, 1);
		}
	}
}