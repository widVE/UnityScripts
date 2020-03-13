using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public static class ListExtensions
	{
		static System.Random RNG = new System.Random();

		public static void Shuffle<T>(this IList<T> list)
		{
			//https://stackoverflow.com/questions/273313/randomize-a-listt
			int i = list.Count;
			while(i > 1)
			{
				i--;
				int j = RNG.Next(i + 1);
				T t = list[j];
				list[j] = list[i];
				list[i] = t;
			}
		}
	}
}