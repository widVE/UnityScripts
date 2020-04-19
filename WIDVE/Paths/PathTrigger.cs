using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace WIDVE.Paths
{
	public class PathTrigger : MonoBehaviour
	{
		[SerializeField]
		PathPosition _position;
		public PathPosition Position
		{
			get => _position;
			set => _position = value;
		}

		float LastPosition = 0;

		public event System.Action<PathObject> OnTrigger;

		public void UpdatePosition(float position)
		{
			//activate all objects between the last position and the new position:
			PathObjectSequence sequence = Position.Sequence;

			//start at the first object located after the last frame's position
			int startIndex = sequence.GetPrevIndex(LastPosition) + 1;

			//end just before the first object located after the current frame's position
			int endIndex = sequence.GetNextIndex(Position.Position);

			//don't trigger anything when disabled
			if(enabled)
			{
				for(int i = startIndex; i < endIndex; i++)
				{
					PathObject pathObject = sequence[i];

					//don't let the trigger trigger itself
					if(pathObject == Position) continue;

					//notify that an object has been triggered
					OnTrigger?.Invoke(pathObject);

					//if the object has a PathEvent, trigger it now
					PathEvent pathEvent = pathObject.GetComponentInChildren<PathEvent>();
					if(pathEvent) pathEvent.Trigger();
				}
			}

			//remember the current position for the next frame
			LastPosition = Position.Position;
		}

		public void Reset()
		{
			//return to the starting point without triggering anything
			enabled = false;
			if(Position) Position.SetPosition(0);
			enabled = true;
		}
		
		void OnEnable()
		{
			Position.OnPositionChanged += UpdatePosition;
		}

		void Start()
		{
			Reset();
		}

		void OnDisable()
		{
			Position.OnPositionChanged -= UpdatePosition;
		}
	}
}