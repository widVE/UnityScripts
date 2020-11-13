using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class MouseLook : MonoBehaviour
    {
        [SerializeField]
        Transform _cameraParent;
        Transform CameraParent => _cameraParent;

        [SerializeField]
        [Range(0, 10)]
        float _speed = 1;
        float Speed => _speed;

        [SerializeField]
        bool _invertX = false;
        bool InvertX => _invertX;

        [SerializeField]
        bool _invertY = false;
        bool InvertY => _invertY;

        [SerializeField]
        ScriptableEvent MenuOpenedEvent;

        [SerializeField]
        ScriptableEvent MenuClosedEvent;

        float XRotation = 0;
        float YRotation = 0;

        bool ShouldLook = true;

        void Look(Vector2 lookVector)
		{
            if(!CameraParent) return;

            XRotation += InvertX ? -lookVector.x : lookVector.x;
            YRotation += InvertY ? lookVector.y : -lookVector.y;

            CameraParent.rotation = Quaternion.Euler(YRotation, XRotation, 0);
        }

        void EnableLook()
		{
            ShouldLook = true;
		}

        void DisableLook()
		{
            ShouldLook = false;
		}

        void OnEnable()
        {
            MenuOpenedEvent.Event += DisableLook;
            MenuClosedEvent.Event += EnableLook;
        }

        void OnDisable()
        {
            MenuOpenedEvent.Event -= DisableLook;
            MenuClosedEvent.Event -= EnableLook;
        }

        void Update()
		{
            if(ShouldLook) Look(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * Speed);
        }
	}
}