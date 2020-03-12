using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace WIDVE.Oculus
{
	[CreateAssetMenu(fileName = nameof(OculusButton), menuName = MENU_NAME + "/" + nameof(OculusButton), order = MENU_ORDER)]
	public class OculusButton : ButtonFloat
	{
		[SerializeField]
		OVRInput.RawButton _ovrButton;
		public OVRInput.RawButton OVRButton
		{
			get => _ovrButton;
			set => _ovrButton = value;
		}

		public override float GetRawValue()
		{
			return OVRInput.Get(OVRButton) ? 1f : 0f;
		}

		public override bool GetHeld()
		{
			return OVRInput.Get(OVRButton);
		}

		public override bool GetDown()
		{
			return OVRInput.GetDown(OVRButton);
		}

		public override bool GetUp()
		{
			return OVRInput.GetUp(OVRButton);
		}
	}
}