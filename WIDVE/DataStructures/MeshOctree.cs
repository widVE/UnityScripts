using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.DataStructures
{
    public class MeshOctree : MonoBehaviour
    {
        public class Vertex : System.IEquatable<Vertex>
		{
			int Index;
			Vector3 Position;

			public Vertex(int index, Vector3 position)
			{
				Index = index;
				Position = position;
			}

			public bool Equals(Vertex other)
			{
				return Index == other.Index;
			}
		}
		
		[SerializeField]
        MeshFilter MeshSource;

		[SerializeField]
		[Range(.01f, 1)]
		float MinHalfSize = .1f;

		Octree<Vertex> _vertices;
		Octree<Vertex> Vertices
		{
			get
			{
				if(_vertices == null)
				{
					if(MeshSource && MeshSource.sharedMesh)
					{
						SetupOctree();
					}
				}
				return _vertices;
			}
		}

		void SetupOctree()
		{
			_vertices = CreateOctree(MeshSource, MinHalfSize);
		}

		public static Octree<Vertex> CreateOctree(MeshFilter meshSource, float minHalfSize)
		{
			Bounds meshBounds = meshSource.sharedMesh.bounds;
			Vector3 octreeCenter = meshSource.transform.TransformPoint(meshBounds.center);
			Vector3 meshSize = meshSource.transform.TransformVector(meshBounds.size);
			float octreeSize = Mathf.Max(meshSize.x, meshSize.y, meshSize.z);
			return new Octree<Vertex>(minHalfSize, octreeCenter, octreeSize / 2);
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(MeshOctree))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				if(EditorGUI.EndChangeCheck())
				{
					foreach(MeshOctree mo in targets)
					{
						mo.SetupOctree();
					}
				}
			}
		}
#endif
	}
}