using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.DataStructures
{
	[ExecuteAlways]
    public class MeshOctree : MonoBehaviour
    {
	
		[SerializeField]
        MeshFilter _sourceMesh;
		public MeshFilter SourceMesh
		{
			get
			{
				if(!_sourceMesh) _sourceMesh = GetComponent<MeshFilter>();
				return _sourceMesh;
			}
			set
			{
				if(value != _sourceMesh)
				{
					//need to remake octree if setting a new mesh
					_sourceMesh = value;
					SetupOctree();
				}
			}
		}

		[SerializeField]
		[Range(.01f, 1)]
		float MinHalfSize = .1f;

		[SerializeField]
		bool LiveUpdate = false;

		[SerializeField]
		[HideInInspector]
		bool DrawBounds = false;

		[SerializeField]
		bool DrawNodes = false;

		[SerializeField]
		bool DrawVertices = false;

		Vector3 LastWorldPosition;

		Quaternion LastWorldRotation;

		Vector3 LastWorldScale;

		Octree<int> _vertices;
		public Octree<int> Vertices
		{
			get
			{
				if(SourceMesh && SourceMesh.sharedMesh)
				{
					//create octree if:
					//	it doesn't already exist
					//	mesh Transform has changed since octree was last created
					if(_vertices == null || 
						SourceMesh.transform.position != LastWorldPosition ||
						SourceMesh.transform.rotation != LastWorldRotation ||
						SourceMesh.transform.lossyScale != LastWorldScale)
					{
						SetupOctree();
					}
				}
				return _vertices;
			}
		}

		public void SetupOctree()
		{
			//create octree with the current settings
			_vertices = CreateOctree(SourceMesh, MinHalfSize);

			//fill it with all mesh points
			Mesh mesh = SourceMesh.sharedMesh;
			for(int i = 0; i < mesh.vertices.Length; i++)
			{
				_vertices.Add(i, SourceMesh.transform.TransformPoint(mesh.vertices[i]));
			}

			//remember transform settings for this octree
			LastWorldPosition = SourceMesh.transform.position;
			LastWorldRotation = SourceMesh.transform.rotation;
			LastWorldScale = SourceMesh.transform.lossyScale;

			//Debug.Log($"Created new octree with {Vertices.NumNodes} nodes, {Vertices.NumItems} items.");
		}

		public static Octree<int> CreateOctree(MeshFilter meshSource, float minHalfSize)
		{
			Bounds meshBounds = meshSource.sharedMesh.bounds;
			Vector3 octreeCenter = meshSource.transform.TransformPoint(meshBounds.center);
			Vector3 meshSize = meshSource.transform.TransformVector(meshBounds.size);
			float octreeSize = Mathf.Max(meshSize.x, meshSize.y, meshSize.z);
			return new Octree<int>(minHalfSize, octreeCenter, octreeSize / 2);
		}

		void LateUpdate()
		{
			if(!gameObject.ExistsInScene()) return;
			if(LiveUpdate && SourceMesh)
			{
				//need to update the octree if the transform changed
				if(SourceMesh.transform.position != LastWorldPosition ||
				   SourceMesh.transform.rotation != LastWorldRotation ||
				   SourceMesh.transform.lossyScale != LastWorldScale)
				{
					SetupOctree();
				}		
			}
		}

		void OnDrawGizmosSelected()
		{
			if(SourceMesh && DrawBounds)
			{
				Gizmos.matrix = SourceMesh.transform.localToWorldMatrix;
				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(SourceMesh.sharedMesh.bounds.center, SourceMesh.sharedMesh.bounds.size);
			}

			Gizmos.matrix = Matrix4x4.identity;
			if(DrawNodes) Vertices.DrawNodeGizmos();
			if(DrawVertices) Vertices.DrawItemGizmos();
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

				if(GUILayout.Button("Create Octree") || EditorGUI.EndChangeCheck())
				{
					foreach(MeshOctree mo in targets)
					{
						mo.SetupOctree();
					}
				}

				MeshOctree meshOctree = target as MeshOctree;
				if(meshOctree.Vertices != null)
				{
					EditorGUILayout.TextField($"Octree with {meshOctree.Vertices.NumNodes} nodes, {meshOctree.Vertices.NumItems} vertices.", EditorStyles.miniLabel);
				}
			}
		}
#endif
	}
}