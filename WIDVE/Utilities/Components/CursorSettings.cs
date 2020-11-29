using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Patterns;

namespace WIDVE.Utilities
{
	public class CursorSettings : MonoBehaviour
	{
		[SerializeField]
		bool _showCursor = false;
		public bool ShowCursor
		{
			get => _showCursor;
			set
			{
				_showCursor = value;
				Cursor.visible = _showCursor;
			}
		}
		
		[SerializeField]
		CursorLockMode _constraints = CursorLockMode.Confined;
		public CursorLockMode Constraints
		{
			get => _constraints;
			set
			{
				_constraints = value;
				Cursor.lockState = _constraints;
			}
		}

		void Start()
		{
			ShowCursor = _showCursor;
			Constraints = _constraints;
		}

		public class Commands
		{
			public class ShowCursor : Command<CursorSettings>
			{
				readonly bool Show;
				bool i_Show;

				public ShowCursor(CursorSettings target, bool show) : base(target)
				{
					Show = show;
				}

				public override void Execute()
				{
					i_Show = Target.ShowCursor;
					Target.ShowCursor = Show;
				}

				public override void Undo()
				{
					Target.ShowCursor = i_Show;
				}
			}
		}
	}
}