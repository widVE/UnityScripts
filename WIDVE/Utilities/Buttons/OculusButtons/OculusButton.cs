using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(OculusButton), menuName = MENU_NAME + "/" + nameof(OculusButton), order = MENU_ORDER)]
	public class OculusButton : ButtonFloat
	{
		[SerializeField]
		OVRInput.RawButton _ovrButton;
		public OVRInput.RawButton OVRButton => _ovrButton;

		public event System.Action OnButtonDown;
		public event System.Action OnButtonUp;

		public override void UpdateInput()
		{
			if (GetDown()) OnButtonDown?.Invoke();
			if (GetUp()) OnButtonUp?.Invoke();
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