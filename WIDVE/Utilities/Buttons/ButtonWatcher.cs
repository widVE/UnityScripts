using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
	[System.Serializable]
	public class ButtonWatcher : UnityEngine.Object
	{
		public enum Modes { Trigger, Time }

		[SerializeField]
		Modes _mode = Modes.Trigger;
		public Modes Mode
		{
			get => _mode;
			set => _mode = value;
		}

		[SerializeField]
		[Range(0, 1)]
		float _threshold = .3f;
		public float Threshold
		{
			get => _threshold;
			set => _threshold = value;
		}

		[SerializeField]
		float _waitTime = 1f;
		public float WaitTime
		{
			get => _waitTime;
			set => _waitTime = value;
		}

		bool HeldThisFrame = false;

		bool TriggeredThisFrame = false;

		bool TriggeredLastFrame = false;

		float LastTriggerTime = 0;

		float GetValue(Button button, int axis)
		{
			if(button is ButtonFloat bf) return bf.GetValue();

			else if(button is ButtonVector2 bv2) return bv2.GetValue()[axis % 2];

			else return 0;
		}

		/// <summary>
		/// UpdateInput should only be called once per frame.
		/// </summary>
		public void UpdateInput(Button button, int axis = 0)
		{
			//update triggered status
			TriggeredLastFrame = TriggeredThisFrame;
			TriggeredThisFrame = false;

			//check current button status
			float value = GetValue(button, axis);
			HeldThisFrame = value > Threshold;

			if(HeldThisFrame)
			{
				//check if the button was freshly triggered
				float time = Time.time;

				switch(Mode)
				{
					default:
					case Modes.Trigger:
						if(!TriggeredLastFrame)
						{
							//trigger if the button wasn't pressed last frame
							TriggeredThisFrame = true;
						}
						break;
					case Modes.Time:
						if(time > LastTriggerTime + WaitTime)
						{
							//trigger if enough time has passed
							TriggeredThisFrame = true;
							LastTriggerTime = Time.time;
						}
						break;
				}
			}
		}

		//call these after calling UpdateInput
		public bool GetHeld()
		{
			return HeldThisFrame;
		}

		public bool GetDown()
		{
			return TriggeredThisFrame && !TriggeredLastFrame;
		}

		public bool GetUp()
		{
			return !TriggeredThisFrame && TriggeredLastFrame;
		}

#if UNITY_EDITOR
		//[CustomPropertyDrawer(typeof(ButtonWatcher))]
		class PropertyDrawer : WIDVEPropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				ButtonWatcher bw = property.objectReferenceValue as ButtonWatcher;

				switch(bw.Mode)
				{
					default:
					case Modes.Trigger:
						return 2 * EditorGUIUtility.singleLineHeight;
					case Modes.Time:
						return 3 * EditorGUIUtility.singleLineHeight;
				}
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				ButtonWatcher bw = property.objectReferenceValue as ButtonWatcher;

				int e = 0;
				EditorGUI.BeginProperty(position, label, property);

				EditorGUI.PropertyField(GetRect(position, e++), property.FindPropertyRelative(nameof(_mode)));
				EditorGUI.PropertyField(GetRect(position, e++), property.FindPropertyRelative(nameof(_threshold)));
				if(bw.Mode == Modes.Time)
				{
					EditorGUI.PropertyField(GetRect(position, e++), property.FindPropertyRelative(nameof(_waitTime)));
				}

				EditorGUI.EndProperty();
			}
		}
#endif
	}
}