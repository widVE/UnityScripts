using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public class LaserPointer : Selector
	{
		[SerializeField]
		float _range = 10f;
		float Range => _range;

		RaycastHit? Cast()
		{
			RaycastHit hit;

			if(Physics.Raycast(transform.position, transform.forward, out hit, Range, Layers)) return hit;

			else return null;
		}

		void Update()
		{
			//cast a ray every frame
			RaycastHit? _hit = Cast();

			//also check the trigger
			bool triggerDown = GetTriggerDown();

			//process the raycast
			if(_hit is RaycastHit hit)
			{
				Collider[] colliders = { hit.collider };

				//highlight anything hit by the ray
				Highlight(colliders);

				//if trigger was pressed, select whatever was hit
				if(triggerDown) Select(colliders);
			}

			else
			{
				//nothing was hit - just update the highlighting
				Highlight(EmptySelection);
			}
		}
	}
}