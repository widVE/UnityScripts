using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WIDVE.Utilities;

namespace WIDVE.Graphics
{
    public abstract class GraphicController : Controller<Graphic>, IInterpolatable
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