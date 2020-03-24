using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(ScriptableEvent), menuName = WIDVEEditor.MENU + "/" + nameof(ScriptableEvent), order = WIDVEEditor.C_ORDER)]
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
			//invoke the event with args
			Event?.Invoke(t);

			//also invoke the base event in case anything is subscribed to that
			base.Invoke();
		}
	}
}