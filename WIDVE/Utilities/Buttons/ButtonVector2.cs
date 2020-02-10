using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public abstract class ButtonVector2 : Button<Vector2>
	{
		public override Vector2 GetValue()
		{
			Vector2 value = Vector2.zero;
			if (Active)
			{
				Vector2 rawValue = GetRawValue();
				for(int i = 0; i < 2; i++)
				{
					value[i] = GetSmoothedFloat(rawValue[i], Smoothing, Multiplier);
				}
			}
			return value;
		}
	}
}