using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using WIDVE.Patterns;
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
		[HideInInspector]
		List<GameObject> _interpolatableObjects;
		List<GameObject> InterpolatableObjects => _interpolatableObjects ?? (_interpolatableObjects = new List<GameObject>());

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

				//access the IInterpolatable interface from attached GameObjects
				if(InterfaceCache.Get<IInterpolatable>(io) is IInterpolatable ii)
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
		/// <param name="value">Target value at end of lerp.</param>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		public void LerpOverTime(float value, float time)
		{
			StopAllCoroutines();

			StartCoroutine(InterpolateOverTime(value, time));
		}

		/// <summary>
		/// Lerp from startValue to endValue over time.
		/// </summary>
		/// <param name="start">Starting value.</param>
		/// <param name="end">Ending value.</param>
		/// <param name="time">Time in seconds to lerp from startValue to endValue.</param>
		public void LerpOverTime(float start, float end, float time)
		{
			StopAllCoroutines();

			SetRawValue(start);

			//in the coroutine, 'time' is really just velocity...
			//...we need to compute a new time value based on the difference between start and end values
			float lerpTime = time / Mathf.Abs(start - end);

			StartCoroutine(InterpolateOverTime(end, lerpTime));
		}

		/// <summary>
		/// Coroutine that lerps RawValue towards targetValue over time.
		/// </summary>
		/// <param name="value">Value to lerp towards.</param>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		IEnumerator InterpolateOverTime(float value, float time)
		{
			if (!Mathf.Approximately(RawValue, value))
			{
				//if time is 0 or values already match, just skip to end
				if (time > 0)
				{
					//lerp raw value over time
					if (RawValue < value)
					{
						//increase over time
						while (RawValue < value)
						{
							float newRawValue = RawValue + (Time.deltaTime / time);
							SetRawValue(Mathf.Min(newRawValue, value));
							yield return null;
						}
					}
					else
					{
						//decrease over time
						while (RawValue > value)
						{
							float newRawValue = RawValue - (Time.deltaTime / time);
							SetRawValue(Mathf.Max(newRawValue, value));
							yield return null;
						}
					}
				}
			}

			//set raw value directly at end
			SetRawValue(value);
		}

		public class Commands
		{
			public class Lerp : Command<Interpolator>
			{
				readonly float? Start;
				readonly float End;
				readonly float Time;
				float i_RawValue;

				/// <summary>
				/// Creates a lerp from current value to the end value over time.
				/// </summary>
				public Lerp(Interpolator target, float end, float time) : base(target)
				{
					Start = null;
					End = end;
					Time = time;
				}

				/// <summary>
				/// Creates a lerp from the start value to the end value over time.
				/// </summary>
				public Lerp(Interpolator target, float start, float end, float time) : this(target, end, time)
				{
					Start = start;
				}

				public override void Execute()
				{
					//store initial interpolator raw value
					i_RawValue = Target.RawValue;

					//start lerping toward the new value
					if(Start is float start) Target.LerpOverTime(start, End, Time);
					else Target.LerpOverTime(End, Time);
				}

				public override void Undo()
				{
					//stop lerping
					Target.StopAllCoroutines();

					//restore original raw value
					Target.SetRawValue(i_RawValue);
				}
			}
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(Interpolator))]
		class Editor : UnityEditor.Editor
		{
			ReorderableList _interpolatableObjects;

			protected virtual void OnEnable()
			{
				_interpolatableObjects = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(_interpolatableObjects)),
											true, true, true, true);

				_interpolatableObjects.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Interpolatable Objects");
				};

				_interpolatableObjects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = _interpolatableObjects.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
											property: element,
											label: new GUIContent(index.ToString()));
				};
			}

			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				serializedObject.Update();

				DrawDefaultInspector();
				_interpolatableObjects.DoLayoutList();

				serializedObject.ApplyModifiedProperties();

				if(EditorGUI.EndChangeCheck())
				{
					foreach(Interpolator i in targets)
					{
						//notify that RawValue has changed
						i.SetRawValue(i.RawValue);
					}
				}
			}
		}
#endif
	}
}