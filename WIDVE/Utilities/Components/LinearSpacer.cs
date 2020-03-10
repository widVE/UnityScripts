using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public class LinearSpacer : Spacer
	{
		protected override bool RotateObjects => false;

		[SerializeField]
		Vector3 _start = new Vector3(0, 0, 0);
		public Vector3 Start
		{
			get => _start;
			set => _start = value;
		}

		[SerializeField]
		Vector3 _end = new Vector3(0, 1, 0);
		public Vector3 End
		{
			get => _end;
			set => _end = value;
		}

		public override Vector3 GetPosition(int index)
		{
			Vector3 start = transform.position + transform.TransformPoint(Start);
			Vector3 end = transform.position + transform.TransformPoint(End);
			return GetPosition(index, Objects.Count, start, end);
		}

		/// <summary>
		/// Returns the world position of the object at index.
		/// </summary>
		/// <param name="index">Object's position in the layout.</param>
		/// <param name="numObjects">Total number of objects in the layout.</param>
		/// <param name="start">World space start position of the layout.</param>
		/// <param name="end">World space end position of the layout.</param>
		/// <returns>World position of the object at index.</returns>
		public static Vector3 GetPosition(int index, int numObjects, Vector3 start, Vector3 end)
		{
			//position is just a simple lerp
			return Vector3.Lerp(start, end, GetTime(index, numObjects));
		}
	}
}