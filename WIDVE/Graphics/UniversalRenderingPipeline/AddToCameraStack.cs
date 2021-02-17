using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics
{
	[ExecuteAlways]
	public class AddToCameraStack : MonoBehaviour
	{
		[SerializeField]
		string MainCameraTag = "MainCamera";

		Camera _mainCamera;
		Camera MainCamera
		{
			get
			{
				if(!_mainCamera)
				{
					GameObject mcgo = GameObject.FindGameObjectWithTag(MainCameraTag);
					if(mcgo) _mainCamera = mcgo.GetComponentInChildren<Camera>();
				}
				return _mainCamera;
			}
		}

		[SerializeField]
		Camera OverlayCamera;

		public bool AddToStack(Camera mainCamera, Camera overlayCamera)
		{
			if(!mainCamera) return false;

			UniversalAdditionalCameraData cameraData = mainCamera.GetUniversalAdditionalCameraData();
			if(cameraData)
			{
				cameraData.cameraStack.Add(overlayCamera);
				return true;
			}
			else return false;
		}

		public bool RemoveFromStack(Camera mainCamera, Camera overlayCamera)
		{
			if(!mainCamera) return false;

			UniversalAdditionalCameraData cameraData = mainCamera.GetUniversalAdditionalCameraData();
			if(cameraData)
			{
				cameraData.cameraStack.Remove(overlayCamera);
				return true;
			}
			else return false;
		}

		void OnEnable()
		{
			if(gameObject.ExistsInScene())
			{
				//add camera to main camera's stack
				AddToStack(MainCamera, OverlayCamera);
			}
		}

		void OnDisable()
		{
			if(gameObject.ExistsInScene())
			{
				//remove camera from camera stack
				RemoveFromStack(MainCamera, OverlayCamera);
			}
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(AddToCameraStack))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				SerializedProperty mainCameraTag = serializedObject.FindProperty(nameof(MainCameraTag));
				mainCameraTag.stringValue = EditorGUILayout.TagField(tag: mainCameraTag.stringValue, label: mainCameraTag.displayName);

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(OverlayCamera)));

				serializedObject.ApplyModifiedProperties();

				GUILayout.BeginHorizontal();

				if(GUILayout.Button("Add"))
				{
					foreach(AddToCameraStack atcs in targets)
					{
						atcs.AddToStack(atcs.MainCamera, atcs.OverlayCamera);
						EditorUtility.SetDirty(atcs.MainCamera);
					}
				}

				if(GUILayout.Button("Remove"))
				{
					foreach(AddToCameraStack atcs in targets)
					{
						atcs.RemoveFromStack(atcs.MainCamera, atcs.OverlayCamera);
						EditorUtility.SetDirty(atcs.MainCamera);
					}
				}

				GUILayout.EndHorizontal();
			}
		}
#endif
	}
}