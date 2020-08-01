using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
using TMPro;

namespace WIDVE.Graphics
{
    public abstract class TMPController : Controller<TMP_Text>, IInterpolatable
    {
		[SerializeField]
		[HideInInspector]
		float _currentValue;
		protected float CurrentValue
		{
			get => _currentValue;
			set => _currentValue = value;
		}

		public bool Enabled => enabled;

		public virtual bool FunctionWhenDisabled => false;

		public abstract void SetValue(float value);
	}
}