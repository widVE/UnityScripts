using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName =nameof(OculusAxis2D), menuName =MENU_NAME + "/" + nameof(OculusAxis2D), order =MENU_ORDER)]
	public class OculusAxis2D : ButtonVector2
	{
		[SerializeField]
		OVRInput.RawAxis2D _ovrAxis;
		public OVRInput.RawAxis2D OVRAxis => _ovrAxis;

		public override Vector2 GetRawValue()
		{
			return OVRInput.Get(OVRAxis);
		}
	}
}