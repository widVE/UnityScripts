using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class MousePointer : Selector
    {
		[SerializeField]
		Camera _playerCam;
		Camera PlayerCam => _playerCam;

		enum CastModes { MousePosition, CenterOfScreen }

		[SerializeField]
		CastModes _castFrom = CastModes.MousePosition;
		CastModes CastFrom => _castFrom;

		RaycastHit? Cast(LayerMask layers)
		{
			if(!PlayerCam)
			{
				Debug.LogError("Can't do a raycast! MousePointer needs a reference to the main player camera.");
				return null;
			}

			Ray ray;
			RaycastHit hit;

			//cast a ray forwards from the camera's near clip plane through the mouse position
			ray = PlayerCam.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit, float.MaxValue, layers)) return hit;

			else return null;
		}

		void ProcessSelection()
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

		void Update()
        {
            ProcessSelection();
        }
    }
}