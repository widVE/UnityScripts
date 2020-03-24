using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Patterns;

namespace WIDVE.Paths
{
	[ExecuteAlways]
	public class PathObserver : Observer<PathCreator>
	{
		[SerializeField]
		PathCreator _path;
		PathCreator Path => _path;

		protected override string ObserveeName => nameof(_path);

		protected override void Subscribe(PathCreator path)
		{
			if(path) path.pathUpdated += Notify;
		}

		protected override void Unsubscribe(PathCreator path)
		{
			if(path) path.pathUpdated -= Notify;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			Subscribe(Path);
		}

		protected override void OnDisable()
		{
			Unsubscribe(Path);
		}
	}
}