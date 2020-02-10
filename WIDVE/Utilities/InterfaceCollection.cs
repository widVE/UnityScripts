using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	/// <summary>
	/// Caches references to interfaces by their GameObject, so GetComponent doesn't need to be called as often.
	/// </summary>
	/// <typeparam name="I">Type of interface to cache.</typeparam>
	public class InterfaceCollection<I> where I : class
	{
		Dictionary<GameObject, I> _interfaces;
		Dictionary<GameObject, I> Interfaces => _interfaces ?? (_interfaces = new Dictionary<GameObject, I>());

		public I Get(GameObject gameObject)
		{
			I i;
			if(!Interfaces.TryGetValue(gameObject, out i))
			{
				i = gameObject.GetComponent<I>();
				if(i != null) Interfaces[gameObject] = i;
			}
			return i;
		}
	}
}