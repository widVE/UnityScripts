using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using WIDVE.Utilities;

namespace WIDVE.Graphics
{ 
	public class Interpolator : MonoBehaviour
	{
		[SerializeField]
		AnimationCurve _smoothing = AnimationCurve.EaseInOut(0, 0, 1, 1);
		protected AnimationCurve Smoothing => _smoothing;

		[SerializeField]
		[Range(0, 1)]
		float _rawValue = 0f;
		/// <summary>
		/// Current interpolation value (between 0 and 1).
		/// </summary>
		public float RawValue => _rawValue;

		/// <summary>
		/// Current smoothed interpolation value (between 0 and 1).
		/// </summary>
		public float Value => Mathf.Clamp01(Smoothing.Evaluate(RawValue));

		[SerializeField]
		List<GameObject> _interpolatableObjects;
		List<GameObject> InterpolatableObjects => _interpolatableObjects ?? (_interpolatableObjects = new List<GameObject>());

		InterfaceCollection<IInterpolatable> _interpolatables;
		InterfaceCollection<IInterpolatable> Interpolatables => _interpolatables ?? (_interpolatables = new InterfaceCollection<IInterpolatable>());

		/// <summary>
		/// Delegate that operates on a single float value.
		/// </summary>
		/// <param name="value">Float between 0 and 1.</param>
		public delegate void ValueChangedDelegate(float value);
		/// <summary>
		/// Called whenever RawValue is changed.
		/// </summary>
		public event ValueChangedDelegate RawValueChanged;
		/// <summary>
		/// Called whenever Value is changed.
		/// </summary>
		public event ValueChangedDelegate ValueChanged;

		/// <summary>
		/// Sets the RawValue of this Interpolator and invokes the value changed events.
		/// </summary>
		public void SetRawValue(float rawValue)
		{
			_rawValue = Mathf.Clamp01(rawValue);

			//invoke value changed events for any scripts that use them
			RawValueChanged?.Invoke(RawValue);
			ValueChanged?.Invoke(Value);

			//let attached objects know that value has changed as well
			for(int i = 0; i < InterpolatableObjects.Count; i++)
			{
				GameObject io = InterpolatableObjects[i];
				if(!io || !io.activeInHierarchy) continue;
				if(Interpolatables.Get(io) is IInterpolatable ii)
				{
					if(!ii.Enabled && !ii.FunctionWhenDisabled) continue;
					ii.SetValue(Value);
				}
			}
		}

		/// <summary>
		/// Lerp from current value to zero over time.
		/// </summary>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		public void LerpTo0(float time) { LerpOverTime(0f, time); }

		/// <summary>
		/// Lerp from current value to one over time.
		/// </summary>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		public void LerpTo1(float time) { LerpOverTime(1f, time); }

		/// <summary>
		/// Lerp from current value to target value over time.
		/// </summary>
		/// <param name="targetValue">Target value at end of lerp.</param>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		public void LerpOverTime(float targetValue, float time)
		{
			StopAllCoroutines();
			StartCoroutine(InterpolateOverTime(targetValue, time));
		}

		/// <summary>
		/// Lerp from startValue to endValue over time.
		/// </summary>
		/// <param name="startValue">Starting value.</param>
		/// <param name="endValue">Ending value.</param>
		/// <param name="time">Time in seconds to lerp from startValue to endValue.</param>
		public void LerpOverTime(float startValue, float endValue, float time)
		{
			StopAllCoroutines();
			SetRawValue(startValue);
			//in the coroutine, 'time' is really just velocity...
			//...we need to compute a new time value based on the difference between start and end values
			float lerpTime = time / Mathf.Abs(startValue - endValue);
			StartCoroutine(InterpolateOverTime(endValue, lerpTime));
		}

		/// <summary>
		/// Coroutine that lerps RawValue towards targetValue over time.
		/// </summary>
		/// <param name="targetValue">Value to lerp towards.</param>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		IEnumerator InterpolateOverTime(float targetValue, float time)
		{   //use RawValue so smoothing doesn't get in the way
			if (!Mathf.Approximately(RawValue, targetValue))
			{
				if (time > 0)
				{	//lerp over time
					float newRawValue;
					if (RawValue < targetValue)
					{   //increase over time
						while (RawValue < targetValue)
						{
							newRawValue = RawValue + (Time.deltaTime / time);
							SetRawValue(Mathf.Min(newRawValue, targetValue));
							yield return null;
						}
					}
					else
					{   //decrease over time
						while (RawValue > targetValue)
						{
							newRawValue = RawValue - (Time.deltaTime / time);
							SetRawValue(Mathf.Max(newRawValue, targetValue));
							yield return null;
						}
					}
				}   //if time is 0 or values already match, just skip to end
			}
			//set value directly at end
			SetRawValue(targetValue);
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(Interpolator))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				if(EditorGUI.EndChangeCheck())
				{
					foreach(Object t in targets)
					{	//notify that RawValue has changed
						Interpolator i = (t as Interpolator);
						i.SetRawValue(i.RawValue);
					}
				}
			}
		}
#endif
	}
}