using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class LockToFloor : MonoBehaviour
    {
        [SerializeField]
        LayerMask _floorLayer;
        LayerMask FloorLayer => _floorLayer;

        [SerializeField]
        float _castDistance = 10;
        float CastDistance => _castDistance;

        [SerializeField]
        BoxCollider _castBox;
        BoxCollider CastBox => _castBox;

        static void MoveToFloor(Transform transform, BoxCollider castBox, float maxDistance, LayerMask floorLayer)
		{
            //perform a cast to find the floor
            RaycastHit hitInfo;
            if(Physics.BoxCast(center: transform.position,
                               halfExtents: transform.TransformVector(castBox.size / 2f),
                               direction: Vector3.down,
                               hitInfo: out hitInfo,
                               orientation: transform.rotation,
                               maxDistance: maxDistance,
                               layerMask: floorLayer))
            {
                //move transform so the center of the cast region is touching the floor
                Vector3 floorPosition = hitInfo.point;
                transform.position = new Vector3(0, floorPosition.y, 0) + transform.TransformPoint(castBox.center);
                transform.position = floorPosition;

                Debug.Log($"Moving to floor ({hitInfo.collider.name})...");
            }
			else
			{
                Debug.Log("Couldn't find a floor!");
			}
		}

		void LateUpdate()
		{
            MoveToFloor(transform, CastBox, CastDistance, FloorLayer);
		}

		void OnDrawGizmosSelected()
		{
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.blue;

            //draw casting box
            Gizmos.DrawWireCube(transform.TransformPoint(CastBox.center), transform.TransformVector(CastBox.size));

            //draw transform lock center
            Gizmos.DrawSphere(transform.TransformPoint(CastBox.center), .05f);
		}
	}
}