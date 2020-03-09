using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Patterns
{
	public interface IObserver<T>
	{
		void OnNotify();
	}
}