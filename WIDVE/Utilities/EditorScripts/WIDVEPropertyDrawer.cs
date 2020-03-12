using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace WIDVE.Utilities
{
	public class WIDVEPropertyDrawer : PropertyDrawer
	{
		/// <summary>
		/// Returns a new Rect at the correct height with single line height.
		/// </summary>
		/// <param name="initRect">Starting position Rect from property drawer.</param>
		/// <param name="e">Element number (controls rect y position).</param>
		protected Rect GetRect(Rect initRect, int e)
		{
			return new Rect(initRect.x,
							initRect.y + (EditorGUIUtility.singleLineHeight * e),
							initRect.width,
							EditorGUIUtility.singleLineHeight);
		}
	}
}
#endif