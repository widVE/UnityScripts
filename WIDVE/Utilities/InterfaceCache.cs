using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	/// <summary>
	/// Caches references to interfaces by their GameObject, so GetComponent doesn't need to be called as often.
	/// </summary>
	public static class InterfaceCache
	{
		class InterfaceMap { }

		class InterfaceMap<I> : InterfaceMap where I : class
		{
			Dictionary<GameObject, I> Interfaces;

			public InterfaceMap()
			{
				Interfaces = new Dictionary<GameObject, I>();
			}

			public I Get(GameObject gameObject)
			{	
				if(!gameObject) return null;

				I i;
				if(!Interfaces.TryGetValue(gameObject, out i))
				{
					//store the interface attached to this GameObject
					i = gameObject.GetComponent<I>();
					if(i != null) Interfaces[gameObject] = i;
				}
				else if(i == null || i.Equals(null))
				{
					//this means the Component implementing this interface has been destroyed
					//see https://answers.unity.com/questions/586144/destroyed-monobehaviour-not-comparing-to-null.html

					//try to get the interface again, in case a new component was added
					i = gameObject.GetComponent<I>();
					if(i == null)
					{
						//remove the interface reference from the cache
						Interfaces.Remove(gameObject);
					}
					else
					{
						//update the cached reference
						Interfaces[gameObject] = i;
					}
				}

				return i;
			}
		}

		static Dictionary<System.Type, InterfaceMap> _maps;
		static Dictionary<System.Type, InterfaceMap> Maps => _maps ?? (_maps = new Dictionary<System.Type, InterfaceMap>());

		static InterfaceMap<I> GetMap<I>() where I : class
		{
			System.Type iType = typeof(I);

			InterfaceMap _map;
			InterfaceMap<I> map;
			if(!Maps.TryGetValue(iType, out _map))
			{
				//create a new generic map and return it
				map = new InterfaceMap<I>();
				Maps[iType] = map;
			}
			else
			{
				//cast to the generic type
				map = _map as InterfaceMap<I>;
			}
			return map;
		}

		/// <summary>
		/// Returns the interface of the specified type attached to this GameObject.
		/// </summary>
		/// <typeparam name="I">Interface type.</typeparam>
		/// <param name="gameObject">GameObject to query.</param>
		/// <returns>Interface instance if found, otherwise null.</returns>
		public static I Get<I>(GameObject gameObject) where I : class
		{
			InterfaceMap<I> map = GetMap<I>();
			return map.Get(gameObject);
		}
	}
}