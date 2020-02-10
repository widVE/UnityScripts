using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Graphics
{
	public interface IInterpolatable
	{
		bool Enabled { get; }
		bool FunctionWhenDisabled { get; }

		void SetValue(float value);
	}
}