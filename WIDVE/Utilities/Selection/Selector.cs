using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public abstract class Selector : MonoBehaviour
    {
        [SerializeField]
        LayerMask _layers;
        protected LayerMask Layers => _layers;

		[SerializeField]
		ButtonFloat _triggerButton;
		ButtonFloat TriggerButton => _triggerButton;

		[SerializeField]
		[Range(0, 1)]
		float _triggerThreshold = .5f;
		float TriggerThreshold => _triggerThreshold;

		bool TriggerHeldLastFrame = false;

		static Collider[] _emptySelection = new Collider[0];
		protected static Collider[] EmptySelection => _emptySelection;

		List<IHighlightable> _currentHighlights;
		protected List<IHighlightable> CurrentHighlights => _currentHighlights ?? (_currentHighlights = new List<IHighlightable>());

		List<T> GetComponentsFromColliders<T>(Collider[] colliders, GameObjectExtensions.SearchModes searchMode) where T : class
		{
			List<T> ts = new List<T>();

			for(int i = 0; i < colliders.Length; i++)
			{
				T t = colliders[i].gameObject.GetComponentInHierarchy<T>(searchMode);

				if(t != null) ts.Add(t);
			}

			return ts;
		}

		public void Highlight(Collider[] colliders)
		{
			//get highlightable objects from colliders
			List<IHighlightable> highlightables = GetComponentsFromColliders<IHighlightable>(colliders, GameObjectExtensions.SearchModes.Parents);

			//check list of currently highlighted objects
			for(int i = CurrentHighlights.Count - 1; i >= 0; i--)
			{
				IHighlightable highlightable = CurrentHighlights[i];

				//remove any highlightables that have been destroyed
				if(highlightable == null)
				{
					CurrentHighlights.RemoveAt(i);
					continue;
				}

				//stop highlighting any objects that aren't being 'selected' anymore
				if(!highlightables.Contains(highlightable))
				{
					highlightable.EndHighlight();
				}
			}

			//highlight any newly 'selected' objects
			for(int i = 0; i < highlightables.Count; i++)
			{
				IHighlightable highlightable = highlightables[i];

				//skip if this object is already being highlighted
				if(CurrentHighlights.Contains(highlightable)) continue;

				//highlight this object
				highlightable.StartHighlight(this);
				CurrentHighlights.Add(highlightable);
			}
		}

		public void Select(Collider[] colliders)
		{
			//select any selectable objects from colliders
			List<ISelectable> selectables = GetComponentsFromColliders<ISelectable>(colliders, GameObjectExtensions.SearchModes.Parents);
			for(int i = 0; i < selectables.Count; i++)
			{
				selectables[i].Select(this);
			}

			//also select any UI Buttons
			List<UnityEngine.UI.Button> buttons = GetComponentsFromColliders<UnityEngine.UI.Button>(colliders, GameObjectExtensions.SearchModes.Parents);
			for(int i = 0; i < buttons.Count; i++)
			{
				buttons[i].onClick?.Invoke();
			}
		}

		protected bool GetTriggerDown()
		{
			bool triggerDown = false;

			if(TriggerButton.GetValue() > TriggerThreshold)
			{
				if(!TriggerHeldLastFrame) triggerDown = true;
				TriggerHeldLastFrame = true;
			}

			else TriggerHeldLastFrame = false;

			return triggerDown;
		}

		protected virtual void OnDisable()
		{
			//remove any highlighting
			Highlight(EmptySelection);
		}
	}
}