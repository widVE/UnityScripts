using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace WIDVE.Oculus
{
	[CreateAssetMenu(fileName = nameof(OculusAxis1D), menuName = nameof(Button) + "/" + nameof(OculusAxis1D), order = B_ORDER)]
	public class OculusAxis1D : ButtonFloat
	{
		[SerializeField]
		OVRInput.RawAxis1D _ovrAxis;
		public OVRInput.RawAxis1D OVRAxis
		{
			get => _ovrAxis;
			set => _ovrAxis = value;
		}

		const float DEADZONE = .1f;

		public override float GetRawValue()
		{
			return OVRInput.Get(OVRAxis);
		}

		public override bool GetHeld()
		{
			return OVRInput.Get(OVRAxis) > DEADZONE;
		}
	}
}