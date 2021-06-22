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

        [SerializeField]
        bool ActiveAtStart = true;

        [SerializeField]
        bool ActiveAtStartEditor = true;

        float XRotation = 0;
        float YRotation = 0;

        bool ShouldLook = true;

        void Look(Vector2 lookVector)
        {
            if (!CameraParent) return;

            XRotation += InvertX ? -lookVector.x : lookVector.x;
            YRotation += InvertY ? lookVector.y : -lookVector.y;

            CameraParent.localRotation = Quaternion.Euler(YRotation, XRotation, 0);
        }

        public void EnableLook()
        {
            ShouldLook = true;
        }

        public void DisableLook()
        {
            ShouldLook = false;
        }

        void OnEnable()
        {
            if (MenuOpenedEvent) MenuOpenedEvent.Event += DisableLook;
            if (MenuClosedEvent) MenuClosedEvent.Event += EnableLook;
        }

        void OnDisable()
        {
            if (MenuOpenedEvent) MenuOpenedEvent.Event -= DisableLook;
            if (MenuClosedEvent) MenuClosedEvent.Event -= EnableLook;
        }

        void Start()
        {
#if UNITY_EDITOR
            if(!ActiveAtStartEditor) DisableLook();
#else
            if (!ActiveAtStart) DisableLook();
#endif
        }

        void Update()
        {
            if (ShouldLook) Look(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * Speed);
        }
    }
}