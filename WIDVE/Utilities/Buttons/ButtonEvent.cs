using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WIDVE.Utilities
{
	public class ButtonEvent : MonoBehaviour
	{
		[SerializeField]
		ButtonFloat _button;
		ButtonFloat Button => _button;

		[SerializeField]
		UnityEvent _event;
		public UnityEvent Event => _event;

		void Update()
		{
			if(!Button) return;

			if(Button.GetDown()) Event?.Invoke();
		}
	}
}