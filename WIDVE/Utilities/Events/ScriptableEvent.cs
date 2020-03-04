using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public class ScriptableEvent : ScriptableObject
	{
		[SerializeField]
		[TextArea]
		string _description;
		string Description => _description;

		public event System.Action Event;

		public void Invoke()
		{
			Event?.Invoke();
		}
	}

	public class ScriptableEvent<T> : ScriptableEvent
	{
		new public event System.Action<T> Event;

		public void Invoke(T t)
		{
			Event?.Invoke(t);
		}
	}
}