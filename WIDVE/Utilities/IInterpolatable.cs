using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public interface IInterpolatable
	{
		bool Enabled { get; }
		bool FunctionWhenDisabled { get; }

		void SetValue(float value);
	}

	public static class IInterpolatableExtensions
	{
		public static bool IsActive(this IInterpolatable ii)
		{
			if(ii.Enabled) return true;
			if(!ii.Enabled && ii.FunctionWhenDisabled) return true;
			return false;
		}
	}
}