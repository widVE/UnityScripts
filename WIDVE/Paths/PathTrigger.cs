using System.Collections;
using System.Collections.Generic;
using System.IO;
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

		[SerializeField]
		[Tooltip("Only events on these layers will be triggered.")]
		LayerMask _layers = ~0;
		public LayerMask Layers
		{
			get => _layers;
			set => _layers = value;
		}

		float LastPosition = 0;

		public event System.Action<PathObject> OnTrigger;

		void ProcessPathObject(PathObject pathObject)
		{
			//skip objects on the wrong layers
			if(!Layers.Contains(pathObject.gameObject.layer)) return;

			//don't let the trigger trigger itself
			if(pathObject == Position) return;

			//if the object has a PathEvent, trigger it now
			IPathEvent[] pathEvents = pathObject.GetComponents<IPathEvent>();
			foreach(IPathEvent ipe in pathEvents)
			{
				ipe.Trigger(this);
			}

			//afterwards, notify that an object has been triggered
			OnTrigger?.Invoke(pathObject);
		}

		public void UpdatePosition(float position)
		{
			if(!enabled) return;
			if(!Application.IsPlaying(this)) return;

			//don't update if position hasn't changed
			if(Mathf.Approximately(position, LastPosition)) return;

			//activate all objects between the last position and the new position
			PathObjectSequence sequence = Position.Sequence;
			List<PathObject> objects = sequence.GetObjects(LastPosition, position);

			if(LastPosition < position)
			{
				//moving forwards
				for(int i = 0; i < objects.Count; i++)
				{
					ProcessPathObject(objects[i]);
				}
			}
			else
			{
				//moving backwards
				for(int i = objects.Count - 1; i >= 0; i--)
				{
					ProcessPathObject(objects[i]);
				}
			}

			//remember the current position for the next frame
			LastPosition = position;
		}

		public void ReturnToStart()
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
			//set the starting position
			LastPosition = Position.Position;
		}

		void OnDisable()
		{
			Position.OnPositionChanged -= UpdatePosition;
		}
	}
}