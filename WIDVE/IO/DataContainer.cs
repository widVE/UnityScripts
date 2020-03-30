using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.IO
{
	/// <summary>
	/// Struct that stores a single piece of readonly data.
	/// <para>Data can be accessed as binary data or as a string.</para>
	/// </summary>
	/// <typeparam name="T">Type of data to store.</typeparam>
	public struct DataContainer
	{
		readonly object _data;
		/// <summary>
		/// The data stored in this container.
		/// </summary>
		public object Data => _data;

		/// <summary>
		/// Creates a new DataContainer storing the specified data.
		/// </summary>
		/// <param name="data">Data to store.</param>
		public DataContainer(object data)
		{
			_data = data;
		}

		/// <summary>
		/// Return Data formatted as a string.
		/// </summary>
		/// <returns>String representing Data.</returns>
		public string DataToString()
		{
			return DataToString(Data);
		}

		/// <summary>
		/// Store the given data in an array of DataContainers.
		/// </summary>
		/// <param name="objects">Objects to store.</param>
		/// <returns>Array of DataContainers.</returns>
		public static DataContainer[] Store(params object[] objects)
		{
			DataContainer[] data = new DataContainer[objects.Length];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = new DataContainer(objects[i]);
			}
			return data;
		}

		/// <summary>
		/// Store each element in the given array in a DataContainer, and store these in a DataContainer array.
		/// </summary>
		/// <param name="objects">Ordered array of objects to store.</param>
		/// <returns>Array of DataContainers.</returns>
		public static DataContainer[] StoreArray<T>(T[] objects)
		{
			DataContainer[] data = new DataContainer[objects.Length];
			for(int i = 0; i < data.Length; i++)
			{
				data[i] = new DataContainer(objects[i]);
			}
			return data;
		}

		/// <summary>
		/// Store each element in the given list in a DataContainer, and store these in a DataContainer array.
		/// </summary>
		/// <param name="objects">Ordered list of objects to store.</param>
		/// <returns>Array of DataContainers.</returns>
		public static DataContainer[] StoreList<T>(List<T> objects)
		{
			DataContainer[] data = new DataContainer[objects.Count];
			for(int i = 0; i < data.Length; i++)
			{
				data[i] = new DataContainer(objects[i]);
			}
			return data;
		}

		//DataToString overrides for various types:
		public static string DataToString(object o)
		{
			return o.ToString();
		}

		public static string DataToString(Component c)
		{
			return c.name;
		}

		public static string DataToString(GameObject go)
		{
			return go.name;
		}

		public static string DataToString(ScriptableObject so)
		{
			return so.name;
		}

		public static string DataToString(string s)
		{
			return s;
		}

		public static string DataToString(Vector2 v2, char sep=' ')
		{
			return $"{v2.x}{sep}{v2.y}";
		}

		public static string DataToString(Vector3 v3, char sep = ' ')
		{
			return $"{v3.x}{sep}{v3.y}{sep}{v3.z}";
		}

		public static string DataToString(Vector4 v4, char sep = ' ')
		{
			return $"{v4.x}{sep}{v4.y}{sep}{v4.z}{sep}{v4.w}";
		}

		public static string DataToString(Quaternion q, char sep = ' ')
		{
			return $"{q.x}{sep}{q.y}{sep}{q.z}{sep}{q.w}";
		}
	}
}