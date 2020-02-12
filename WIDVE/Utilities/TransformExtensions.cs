using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public static class TransformExtensions
	{
		/// <summary>
		/// Instantiates a new prefab instance as a child of this Transform.
		/// </summary>
		/// <param name="keepPrefabConnection">When true, keeps prefab connection (only used in Edit mode).</param>
		public static Object InstantiatePrefab(this Transform transform, Object prefab, bool keepPrefabConnection = true, bool keepPrefabTransform = false)
		{
			return transform.gameObject.InstantiatePrefab(prefab, keepPrefabConnection, keepPrefabTransform);
		}

		/// <summary>
		/// Returns the smallest value from this Transform's X, Y, and Z world scale axes.
		/// </summary>
		public static float GetMinWorldScale(this Transform transform)
		{
			return Mathf.Min(transform.lossyScale.x,
							 transform.lossyScale.y,
							 transform.lossyScale.z);
		}

		/// <summary>
		/// Returns the largest value from this Transform's X, Y, and Z world scale axes.
		/// </summary>
		public static float GetMaxWorldScale(this Transform transform)
		{
			return Mathf.Max(transform.lossyScale.x,
							 transform.lossyScale.y,
							 transform.lossyScale.z);
		}

		/// <summary>
		/// Sets the local scale of this Transform to the given value.
		/// </summary>
		public static void SetLocalScale(this Transform transform, float localScale)
		{
			transform.localScale = localScale * Vector3.one;
		}

		/// <summary>
		/// Sets all three axes of this Transform's world scale to the given value.
		/// </summary>
		public static void SetWorldScale(this Transform transform, float worldScale)
		{
			transform.SetWorldScale(worldScale * Vector3.one);
		}

		/// <summary>
		/// Sets the world scale of this Transform to the given value.
		/// </summary>
		public static void SetWorldScale(this Transform transform, Vector3 worldScale)
		{
			Vector3 initLocalScale = transform.localScale;
			Vector3 initWorldScale = transform.lossyScale;
			Vector3 adjustedLocalScale = new Vector3();
			for(int i = 0; i < 3; i++)
			{
				adjustedLocalScale[i] = worldScale[i] * (initLocalScale[i] / initWorldScale[i]);
			}
			transform.localScale = adjustedLocalScale;
		}
	}
}