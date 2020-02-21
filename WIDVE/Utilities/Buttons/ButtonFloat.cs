using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	/// <summary>
	/// Buttons return a value between -1 and 1, based on current input.
	/// </summary>
	public abstract class ButtonFloat : Button<float>
	{
		public override float GetValue()
		{
			float value = 0f;
			if(Active)
			{
				float rawValue = GetRawValue();
				value = GetSmoothedFloat(rawValue, Smoothing, Multiplier);
			}
			return value;
		}

		protected int LastPollFrame = 0;

		protected enum States { NotHeld, Held }
		protected States LastPollState = States.NotHeld;

		protected virtual void UpdateState() { } //abstract...

		public virtual bool GetHeld() { return false; }

		public virtual bool GetDown() { return false; }

		public virtual bool GetUp() { return false; }
	}
}