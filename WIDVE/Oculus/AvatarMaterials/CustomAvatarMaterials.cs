using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Oculus
{
    //needs to run after Oculus sets its own materials
    [DefaultExecutionOrder(1)]
    public class CustomAvatarMaterials : MonoBehaviour
    {
        const string HAND_LEFT = "hand_left";
        const string HAND_RIGHT = "hand_right";
        const string CONTROLLER_LEFT = "controller_left";
        const string CONTROLLER_RIGHT = "controller_right";

        [SerializeField]
        GameObject _localAvatar;
        GameObject LocalAvatar => _localAvatar;

        [SerializeField]
        Material _handMaterial;
        Material HandMaterial => _handMaterial;

        [SerializeField]
        Material _controllerMaterial;
        Material ControllerMaterial => _controllerMaterial;

        [SerializeField]
        string _layer = "Default";
        string Layer => _layer;

        //these will be true once the materials have been changed
        bool SetLeftHand = false;
        bool SetRightHand = false;
        bool SetLeftController = false;
        bool SetRightController = false;

        //true if any materials still need to be set
        bool SomethingUnset => (HandMaterial && (!SetLeftHand || !SetRightHand)) ||
                               (ControllerMaterial && (!SetLeftController || !SetRightController));

        [ContextMenu("Set Materials")]
        public void SetMaterials()
		{
			if(!LocalAvatar)
			{
                Debug.Log("Can't set materials without a local avatar parent!");
                return;
			}

			if(HandMaterial)
			{
                //set hands
                Transform leftHand = LocalAvatar.transform.Find(HAND_LEFT);
                if(leftHand)
                {
                    Set(leftHand, HandMaterial, Layer);
                    SetLeftHand = true;
                }

                Transform rightHand = LocalAvatar.transform.Find(HAND_RIGHT);
                if(rightHand)
                {
                    Set(rightHand, HandMaterial, Layer);
                    SetRightHand = true;
                }
            }

			if(ControllerMaterial)
			{
                //set controllers
                Transform leftController = LocalAvatar.transform.Find(CONTROLLER_LEFT);
                if(leftController)
                {
                    Set(leftController, ControllerMaterial, Layer);
                    SetLeftController = true;
                }

                Transform rightController = LocalAvatar.transform.Find(CONTROLLER_RIGHT);
                if(rightController)
                {
                    Set(rightController, ControllerMaterial, Layer);
                    SetRightController = true;
                }
            }
		}

        static void Set(Transform transform, Material material, string layer)
		{
            //apply the material to all renderers under this transform
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>(true);
            foreach(Renderer r in renderers)
            {
                r.sharedMaterial = material;
                r.gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }

		void LateUpdate()
		{
			if(SomethingUnset)
			{
                SetMaterials();
			}
			else
			{
                //all done
                enabled = false;
			}
		}
	}
}