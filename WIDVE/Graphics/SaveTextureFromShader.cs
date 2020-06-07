using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics {
    public class SaveTextureFromShader : MonoBehaviour
    {
        [SerializeField]
        ShaderProperties _properties;
        ShaderProperties Properties => _properties;

        [SerializeField]
        string _targetTexture;
        string TargetTexture => _targetTexture;

        [SerializeField]
        GameObject _targetObject;
        GameObject TargetObject => _targetObject;

        [SerializeField]
        DataFilePNG _saveFile;
        DataFilePNG SaveFile => _saveFile;

        void Save()
		{
            //get the renderer and material
            Renderer r = TargetObject.GetComponentInChildren<Renderer>();
			if(!r)
			{
                Debug.Log($"Error! Cannot find Renderer for {TargetObject}.");
                return;
			}

            Material m = r.sharedMaterial;

            //get texture
            Texture texture = m.GetTexture(TargetTexture);

            //save texture to file
            SaveFile.Save(texture);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            Debug.Log($"Saved texture to {SaveFile.Path}.");
		}

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(SaveTextureFromShader))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                SerializedProperty properties = serializedObject.FindProperty(nameof(_properties));
                EditorGUILayout.PropertyField(properties);

                ShaderProperties shaderProperties = properties.objectReferenceValue as ShaderProperties;
                if(shaderProperties)
                {
                    SerializedProperty targetTexture = serializedObject.FindProperty(nameof(_targetTexture));
                    targetTexture.stringValue = shaderProperties.DrawPropertyMenu(ShaderProperties.PropertyTypes.Texture, targetTexture.stringValue, "Target Texture");
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_targetObject)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_saveFile)));

                serializedObject.ApplyModifiedProperties();

                if(GUILayout.Button("Save"))
                {
                    foreach(SaveTextureFromShader stfs in targets)
                    {
                        stfs.Save();
                    }
                }
            }
        }
#endif
    }
}