using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Utilities;
using WIDVE.Patterns;

namespace WIDVE.Paths
{
	public class PathSpacer : Spacer, IObserver<PathCreator>
	{
		protected override bool RotateObjects => true;

		[SerializeField]
		PathCreator _path;
		public PathCreator Path
		{
			get => _path;
			set => _path = value;
		}

		public override Vector3 GetPosition(int index)
		{
			if(!Path) return transform.position;

			return GetPosition(index, NumObjects, Path);
		}

		public static Vector3 GetPosition(int index, int numObjects, PathCreator path)
		{
			float t = GetTime(index, numObjects);

			EndOfPathInstruction endInstruction = path.path.isClosedLoop ? EndOfPathInstruction.Loop : EndOfPathInstruction.Stop;

			Vector3 pathLocalPoint = path.path.GetPointAtTime(t, endInstruction);

			return path.transform.TransformPoint(pathLocalPoint);
		}

		public override Quaternion GetRotation(int index)
		{
			if(!Path) return Quaternion.identity;

			return GetRotation(index, NumObjects, Path);
		}

		public static Quaternion GetRotation(int index, int numObjects, PathCreator path)
		{
			float t = GetTime(index, numObjects);

			EndOfPathInstruction endInstruction = path.path.isClosedLoop ? EndOfPathInstruction.Loop : EndOfPathInstruction.Stop;

			return path.path.GetRotation(t, endInstruction);
		}

		void SetPathPositions()
		{
			//update the path position on objects that have them
			List<Transform> objects = GetObjects();
			for(int i = 0; i < objects.Count; i++)
			{
				PathPosition pathPosition = objects[i].GetComponentInChildren<PathPosition>();
				if(!pathPosition) continue;
				pathPosition.SetPosition(GetTime(i, objects.Count));
			}
		}

		public override void LayoutObjects()
		{
			base.LayoutObjects();

			SetPathPositions();
		}

		public void OnNotify()
		{
			LayoutObjects();
		}
	}
}