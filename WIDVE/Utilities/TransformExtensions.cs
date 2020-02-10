using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public static class TransformExtensions
	{
		public static Object InstantiatePrefab(this Transform transform, Object prefab, bool keepPrefabConnection = true, bool keepPrefabTransform = false)
		{
			return transform.gameObject.InstantiatePrefab(prefab, keepPrefabConnection, keepPrefabTransform);
		}

		public static float GetMinWorldScale(this Transform transform)
		{
			return Mathf.Min(transform.lossyScale.x,
							 transform.lossyScale.y,
							 transform.lossyScale.z);
		}

		public static float GetMaxWorldScale(this Transform transform)
		{
			return Mathf.Max(transform.lossyScale.x,
							 transform.lossyScale.y,
							 transform.lossyScale.z);
		}

		public static void SetLocalScale(this Transform transform, float localScale)
		{
			transform.localScale = localScale * Vector3.one;
		}

		public static void SetWorldScale(this Transform transform, float worldScale)
		{
			transform.SetWorldScale(worldScale * Vector3.one);
		}

		public static void SetWorldScale(this Transform transform, Vector3 worldScale)
		{
			Vector3 initLocalScale = transform.localScale;
			Vector3 initWorldScale = transform.lossyScale;
			Vector3 adjustedLocalScale = Vector3.one;
			for(int i = 0; i < 3; i++)
			{
				adjustedLocalScale[i] = worldScale[i] * (initLocalScale[i] / initWorldScale[i]);
			}
			transform.localScale = adjustedLocalScale;
		}
	}
}