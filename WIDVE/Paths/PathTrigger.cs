using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Graphics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
	public class PathTrigger : MonoBehaviour, IInterpolatable
	{
		[SerializeField]
		PathPosition _position;
		public PathPosition Position
		{
			get => _position;
			set => _position = value;
		}

		[SerializeField]
		Vector2 _range = new Vector2(0, 1);
		public Vector2 Range
		{
			get => _range;
			set => _range = value;
		}

		[SerializeField]
		PathEventSequence _events;
		public PathEventSequence Events
		{
			get => _events;
			set => _events = value;
		}

		float Min => Range[0];

		float Max => Range[1];

		float LastPosition = -1;

		int LastEvent = 0;

		public bool Enabled => enabled;

		public bool FunctionWhenDisabled => false;

		public void SetPosition(float position)
		{
			//clamp position to the range and check that the trigger will actually move
			position = Mathf.Clamp(position, Min, Max);
			if(Mathf.Approximately(position, Position.Position)) return;

			//move to the new position
			Position.SetPosition(position);

			//activate all events between the last position and the new position
			for(int i = LastEvent; i < Events.Count; i++)
			{
				//events should already be sorted by position
				PathEvent pathEvent = Events[i];

				//skip or stop if outside range
				if(pathEvent.Position < Min) continue;
				if(pathEvent.Position > Max) break;

				//stop if exceeding the current position
				if(pathEvent.Position > position) break;

				//trigger the event
				pathEvent.Trigger();
				LastEvent = i;
			}

			//store last position
			LastPosition = position;
		}

		public void SetValue(float value)
		{
			SetPosition(value);
		}

		public void Reset()
		{
			//return to the starting point of the range
			if(Position) Position.SetPosition(Min);
			LastPosition = Min;
			LastEvent = 0;
		}

		void Start()
		{
			Reset();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(PathTrigger))]
		class Editor: UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				if(EditorGUI.EndChangeCheck())
				{
					foreach(PathTrigger pt in targets)
					{
						//validate range and reset
						if(pt.Min < 0) pt._range[0] = 0;
						if(pt.Min > 1) pt._range[0] = 1;
						if(pt.Max < 0) pt._range[1] = 0;
						if(pt.Max > 1) pt._range[1] = 1;

						pt.Reset();
					}
				}
			}
		}
#endif
	}
}