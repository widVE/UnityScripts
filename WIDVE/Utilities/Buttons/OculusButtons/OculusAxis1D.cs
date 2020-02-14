using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(OculusAxis1D), menuName = MENU_NAME + "/" + nameof(OculusAxis1D), order = MENU_ORDER)]
	public class OculusAxis1D : ButtonFloat
	{
		[SerializeField]
		OVRInput.RawAxis1D _ovrAxis;
		public OVRInput.RawAxis1D OVRAxis
		{
			get => _ovrAxis;
			set => _ovrAxis = value;
		}

		public override float GetRawValue()
		{
			return OVRInput.Get(OVRAxis);
		}
	}
}