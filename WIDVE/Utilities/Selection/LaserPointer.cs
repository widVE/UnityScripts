﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public class LaserPointer : Selector
	{
		[SerializeField]
		float _range = 10f;
		float Range => _range;

		RaycastHit? Cast(LayerMask layers)
		{
			RaycastHit hit;

			if(Physics.Raycast(transform.position, transform.forward, out hit, Range, layers)) return hit;

			else return null;
		}

		void Update()
		{
			//cast rays every frame
			RaycastHit? _selectHit = Cast(Layers);

			RaycastHit? _highlightHit;
			if(SeparateHighlightLayers) _highlightHit = Cast(HighlightLayers);
			else _highlightHit = _selectHit;

			//also check the trigger
			bool triggerDown = GetTriggerDown();

			//process highlights
			if(_highlightHit is RaycastHit highlightHit)
			{
				Collider[] colliders = { highlightHit.collider };

				//highlight anything hit by the ray
				Highlight(colliders);
			}
			else
			{
				//nothing to highlight this frame
				Highlight(EmptySelection);
			}

			//process selection
			if(_selectHit is RaycastHit selectHit)
			{
				Collider[] colliders = { selectHit.collider };

				//if trigger was pressed, select whatever was hit
				if(triggerDown)
				{
					Select(colliders);
				}
			}

		}
	}
}