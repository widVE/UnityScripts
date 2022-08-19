//Copyright WID Virtual Environments Group 2018-Present
//Simon Smith
//Ross Tredinnick

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WIDVE.Utilities
{
    public class OnHighlight : MonoBehaviour, IHighlightable
    {
        [SerializeField]
        UnityEvent _onHighlightEvent;
        UnityEvent OnHighlightEvent => _onHighlightEvent;
		
		[SerializeField]
		UnityEvent _onEndHighlightEvent;
		UnityEvent OnEndHighlightEvent => _onEndHighlightEvent;
		
        public void StartHighlight(Selector selector)
        {
            OnHighlightEvent?.Invoke();
        }
		
		public void EndHighlight()
		{
			OnEndHighlightEvent?.Invoke();
		}
    }
}