using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace WIDVE.Utilities
{
    public class SimpleScreenshot : MonoBehaviour
    {
        [SerializeField]
        Camera _screenshotCamera;
        Camera ScreenshotCamera
        {
            get
            {
                if(!_screenshotCamera) _screenshotCamera = GetComponentInChildren<Camera>();
                if(!_screenshotCamera) _screenshotCamera = gameObject.AddComponent<Camera>();
                return _screenshotCamera;
            }
        }

        [SerializeField]
        DataFilePNG _screenshotFile;
        DataFilePNG ScreenshotFile => _screenshotFile;

        [SerializeField]
        RenderTexture _screenshotTexture;
        RenderTexture ScreenshotTexture => _screenshotTexture;

        void UpdateCameraSettings()
        {
            ScreenshotCamera.targetTexture = ScreenshotTexture;
        }

        public void SaveScreenshot()
        {
#if UNITY_EDITOR
            InternalEditorUtility.SetShowGizmos(false);
#endif
            UpdateCameraSettings();

            ScreenshotCamera.Render();

            ScreenshotFile.Save(ScreenshotTexture);

#if UNITY_EDITOR
            InternalEditorUtility.SetShowGizmos(true);

            AssetDatabase.Refresh();

            Debug.Log($"Saved screenshot to {ScreenshotFile.RelativePath}");
#endif
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(SimpleScreenshot))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

                bool changed = EditorGUI.EndChangeCheck();

                if(changed)
                {
                    foreach(SimpleScreenshot ss in targets)
                    {
                        ss.UpdateCameraSettings();
                        EditorUtility.SetDirty(ss.ScreenshotCamera);
                    }
                }

                if(GUILayout.Button("Save Screenshot"))
                {
                    foreach(SimpleScreenshot ss in targets)
                    {
                        ss.SaveScreenshot();
                    }
                }
            }
        }
#endif
    }
}