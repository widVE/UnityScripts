using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.DataStructures
{
    public class MeshOctreeTester : MonoBehaviour
    {
        [SerializeField]
        MeshOctree MeshOctree;

        [SerializeField]
        GameObject TestObject;

        [SerializeField]
        bool DrawSearchGizmos = true;

        Vector3 GetClosestPoint()
		{
            int cvIndex = MeshOctree.Vertices.GetClosestItem(TestObject.transform.position, DrawSearchGizmos);
            Transform meshTransform = MeshOctree.SourceMesh.transform;
            Vector3 vertex = MeshOctree.SourceMesh.sharedMesh.vertices[cvIndex];
            return meshTransform.TransformPoint(vertex);
		}

		void OnDrawGizmosSelected()
		{
            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = Color.gray;
            Vector3 closestPoint = GetClosestPoint();

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(closestPoint, .15f);
		}
	}
}