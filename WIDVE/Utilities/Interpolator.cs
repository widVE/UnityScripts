using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using WIDVE.Patterns;

namespace WIDVE.Utilities
{
	[ExecuteAlways]
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
		public float Value => EvaluateValue(RawValue);

		[SerializeField]
		[HideInInspector]
		List<GameObject> _interpolatableObjects;
		List<GameObject> InterpolatableObjects => _interpolatableObjects ?? (_interpolatableObjects = new List<GameObject>());

		IEnumerator ActiveLerp = null;

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

		float EvaluateValue(float value)
		{
			return Mathf.Clamp01(Smoothing.Evaluate(value));
		}

		/// <summary>
		/// Sets the RawValue of this Interpolator and invokes the value changed events.
		/// </summary>
		public void SetRawValue(float rawValue, bool notify = true)
		{
			_rawValue = Mathf.Clamp01(rawValue);

			if(notify)
			{
				//invoke value changed events for any scripts that use them
				RawValueChanged?.Invoke(RawValue);
				ValueChanged?.Invoke(Value);

				//let attached objects know that value has changed as well
				for(int i = 0; i < InterpolatableObjects.Count; i++)
				{
					GameObject io = InterpolatableObjects[i];
					if(!io || !io.activeInHierarchy) continue;

					//set value on all IInterpolatable components from the attached GameObject
					foreach(IInterpolatable ii in io.GetComponents<IInterpolatable>())
					{
						//skip disabled objects
						if(!ii.IsActive()) continue;

						//set the value!
						ii.SetValue(Value);
					}
				}
			}
		}

		/// <summary>
		/// Lerp from current value to zero over time.
		/// </summary>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		public ICommand LerpTo0(float time) { return LerpOverTime(0f, time); }

		/// <summary>
		/// Lerp from current value to one over time.
		/// </summary>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		public ICommand LerpTo1(float time) { return LerpOverTime(1f, time); }

		/// <summary>
		/// Lerp from current value to target value over time.
		/// </summary>
		/// <param name="value">Target value at end of lerp.</param>
		/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
		public ICommand LerpOverTime(float value, float time)
		{
			ICommand l = new Commands.Lerp(this, value, time);
			l.Execute();
			return l;
		}

		/// <summary>
		/// Lerp from startValue to endValue over time.
		/// </summary>
		/// <param name="start">Starting value.</param>
		/// <param name="end">Ending value.</param>
		/// <param name="time">Time in seconds to lerp from startValue to endValue.</param>
		public ICommand LerpOverTime(float start, float end, float time)
		{
			ICommand l = new Commands.Lerp(this, start, end, time);
			l.Execute();
			return l;
		}

		/// <summary>
		/// Stops any current lerp in progress.
		/// </summary>
		public void StopLerp()
		{
			if(ActiveLerp != null) StopCoroutine(ActiveLerp);
		}
		
		public bool LerpActive()
		{
			return (ActiveLerp != null);
		}

		public class Commands
		{
			public class Lerp : Command<Interpolator>
			{
				readonly float? Start;
				readonly float End;
				readonly float Time;
				float i_RawValue;
				IEnumerator LerpIEnumerator;

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
					//store initial raw value
					i_RawValue = Target.RawValue;

					//stop any lerp in progress
					Target.StopLerp();

					//compute lerp time based on starting value
					float time;
					if(Start is float start)
					{
						//in the coroutine, 'time' is really just velocity...
						//...we need to compute a new time value based on the difference between start and end values
						time = Time / Mathf.Abs(start - End);

						//also set the start value now
						Target.SetRawValue(start);
					}
					else
					{
						//starting from current value - just use the unmodified time
						time = Time;
					}

					//perform lerp
					if(!Application.isPlaying || !Target.gameObject.activeInHierarchy)
					{
						//no coroutines in edit mode or if target's GameObject is disabled
						//just set the value for testing
						Target.SetRawValue(End);
					}
					else
					{
						LerpIEnumerator = InterpolateOverTime(End, time);
						Target.ActiveLerp = LerpIEnumerator;
						Target.StartCoroutine(LerpIEnumerator);
					}
				}

				public override void Undo()
				{
					if(LerpIEnumerator == null || Target.ActiveLerp == LerpIEnumerator)
					{
						//stop lerping
						Target.StopLerp();

						//restore original raw value
						Target.SetRawValue(i_RawValue);
					}
				}

				/// <summary>
				/// Coroutine that lerps RawValue towards targetValue over time.
				/// </summary>
				/// <param name="value">Value to lerp towards.</param>
				/// <param name="time">Time in seconds to lerp from 0 to 1.</param>
				IEnumerator InterpolateOverTime(float value, float time)
				{
					if(!Mathf.Approximately(Target.RawValue, value))
					{
						//if time is 0 or values already match, just skip to end
						if(time > 0)
						{
							//lerp raw value over time
							if(Target.RawValue < value)
							{
								//increase over time
								while(Target.RawValue < value)
								{
									float newRawValue = Target.RawValue + (UnityEngine.Time.deltaTime / time);
									Target.SetRawValue(Mathf.Min(newRawValue, value));
									yield return null;
								}
							}
							else
							{
								//decrease over time
								while(Target.RawValue > value)
								{
									float newRawValue = Target.RawValue - (UnityEngine.Time.deltaTime / time);
									Target.SetRawValue(Mathf.Max(newRawValue, value));
									yield return null;
								}
							}
						}
					}

					//set raw value directly at end
					Target.SetRawValue(value);

					//done lerping
					Target.ActiveLerp = null;
				}
			}
		}

		void OnEnable()
		{
#if UNITY_EDITOR
			PrefabUtility.prefabInstanceUpdated += RefreshValue;
#endif
			//update value when enabled
			SetRawValue(RawValue);
		}

#if UNITY_EDITOR
		void RefreshValue(GameObject instance)
		{
			SetRawValue(RawValue);
		}

		void OnDisable()
		{
			PrefabUtility.prefabInstanceUpdated -= RefreshValue;
		}

		[CanEditMultipleObjects]
		[CustomEditor(typeof(Interpolator))]
		class Editor : UnityEditor.Editor
		{
			ReorderableList InterpolatableObjects;

			protected virtual void OnEnable()
			{
				InterpolatableObjects = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(_interpolatableObjects)),
											true, true, true, true);

				InterpolatableObjects.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Objects");
				};

				InterpolatableObjects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = InterpolatableObjects.serializedProperty.GetArrayElementAtIndex(index);
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
				InterpolatableObjects.DoLayoutList();

				serializedObject.ApplyModifiedProperties();

				if(EditorGUI.EndChangeCheck())
				{
					foreach(Interpolator i in targets)
					{
						//notify that value has changed
						i.SetRawValue(i.RawValue);
					}
				}
			}
		}
#endif
	}
}