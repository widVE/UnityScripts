using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace WIDVE.Oculus
{
	[CreateAssetMenu(fileName =nameof(OculusAxis2D), menuName = nameof(Button) + "/" + nameof(OculusAxis2D), order =B_ORDER)]
	public class OculusAxis2D : ButtonVector2
	{
		[SerializeField]
		OVRInput.RawAxis2D _ovrAxis;
		public OVRInput.RawAxis2D OVRAxis
		{
			get => _ovrAxis;
			set => _ovrAxis = value;
		}

		public override Vector2 GetRawValue()
		{
			return OVRInput.Get(OVRAxis);
		}
	}
}