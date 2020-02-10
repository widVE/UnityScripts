using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	}
}