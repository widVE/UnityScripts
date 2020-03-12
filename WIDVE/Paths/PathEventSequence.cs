using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Patterns;

namespace WIDVE.Paths
{
	public class PathEventSequence : MonoBehaviour, IObserver<PathCreator>, IEnumerable
	{
		[SerializeField]
		PathCreator _path;
		PathCreator Path => _path;

		List<PathEvent> _events;
		public List<PathEvent> Events
		{
			get =>	_events ?? (_events = GetEvents());
			private set => _events = value;
		}

		public PathEvent this[int index] => Events[index];

		public int Count => Events.Count;

		List<PathEvent> GetEvents()
		{
			//add all path events in child objects
			PathEvent[] allPathEvents = GetComponentsInChildren<PathEvent>(true);
			List<PathEvent> pathEvents = new List<PathEvent>(allPathEvents.Length);
			for(int i = 0; i < allPathEvents.Length; i++)
			{
				pathEvents.Add(allPathEvents[i]);
			}

			//sort path events by their position
			pathEvents.Sort((x, y) => x.Position.CompareTo(y.Position));

			return pathEvents;
		}

		public void UpdateEvents()
		{
			Events = GetEvents();
		}

		public void OnNotify()
		{
			UpdateEvents();
		}

		public IEnumerator GetEnumerator()
		{
			for(int i = 0; i < Events.Count; i++)
			{
				yield return Events[i];
			}
		}
	}
}