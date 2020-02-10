using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	//[CreateAssetMenu(fileName = nameof(Button), menuName = MENU_NAME + "/" + nameof(Button), order = MENU_ORDER)]
	public abstract class Button : ScriptableObject
	{
		protected const string MENU_NAME = "Button";
		protected const int MENU_ORDER = 2000;

		[SerializeField]
		AnimationCurve _smoothing = AnimationCurve.EaseInOut(0, 0, 1, 1);
		protected AnimationCurve Smoothing => _smoothing;

		[SerializeField]
		[Range(-1f, 1f)]
		float _multiplier = 1f;
		protected float Multiplier => _multiplier;

		[SerializeField]
		bool _active = true;
		/// <summary>
		/// When inactive, value will always return 0.
		/// </summary>
		public bool Active
		{
			get => _active;
			set => _active = value;
		}

		/// <summary>
		/// Have a MonoBehaviour call this during Update to check for input changes.
		/// </summary>
		public virtual void UpdateInput() { }

		protected static float GetSmoothedFloat(float rawValue, AnimationCurve smoothing, float multiplier)
		{
			float value = Mathf.Abs(smoothing.Evaluate(Mathf.Abs(rawValue)));
			value = Mathf.Clamp01(value);
			value *= Mathf.Sign(rawValue);
			value *= multiplier;
			return value;
		}
	}

	public abstract class Button<T> : Button where T : struct
	{
		/// <summary>
		/// Returns current smoothed button value.
		/// <para>Note: this is *not* scaled by deltaTime.</para>
		/// </summary>
		public abstract T GetValue();

		/// <summary>
		/// Returns current unsmoothed button value.
		/// <para>Note: this is not affected by the Active toggle.</para>
		/// </summary>
		public abstract T GetRawValue();	
	}
}