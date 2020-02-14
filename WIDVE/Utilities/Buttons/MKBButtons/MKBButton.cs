using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(MKBButton), menuName = MENU_NAME + "/" + nameof(MKBButton), order = MENU_ORDER)]
	public class MKBButton : ButtonFloat
	{
		[SerializeField]
		KeyCode _key;
		public KeyCode Key
		{
			get => _key;
			set => _key = value;
		}

		public override float GetRawValue()
		{
			return Input.GetKey(Key) ? 1f : 0f;
		}

		public override bool GetHeld()
		{
			return Input.GetKey(Key); 
		}

		public override bool GetUp()
		{
			return Input.GetKeyUp(Key);
		}

		public override bool GetDown()
		{
			return Input.GetKeyDown(Key);
		}
	}
}