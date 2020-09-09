using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class LaserPointerVisual : MonoBehaviour
    {
        [SerializeField]
        LineRenderer _line;
        LineRenderer Line => _line;

        [SerializeField]
        Material _hitMaterial;
        Material HitMaterial => _hitMaterial;

        [SerializeField]
        Material _noHitMaterial;
        Material NoHitMaterial => _noHitMaterial;

        [SerializeField]
        bool _extendToTarget = true;
        bool ExtendToTarget => _extendToTarget;

        [SerializeField]
        [Range(0, 20)]
        float _lineLength = 5;
        float LineLength => _lineLength;

        Vector3 LineEndPoint => new Vector3(0, 0, LineLength);

        public void UpdateVisual(RaycastHit? hit)
		{
			if(hit != null)
			{
                //update material
                Line.sharedMaterial = HitMaterial;

                if(ExtendToTarget)
                {
                    //extend laser to target
                    Vector3 hitLocalPoint = Line.transform.InverseTransformPoint(hit.Value.point);
                    Line.SetPosition(1, hitLocalPoint);
                }
            }
			else
			{
                //nothing was hit
                Line.sharedMaterial = NoHitMaterial;

                if(ExtendToTarget)
                {
                    //reset line length
                    Line.SetPosition(1, LineEndPoint);
                }
            }
		}

		void OnValidate()
		{       
            //match line length if it is changed
            if(Line && Line.GetPosition(1) != LineEndPoint)
			{
                Line.SetPosition(1, LineEndPoint);
            }
		}
	}
}