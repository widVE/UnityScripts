using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics
{
    [ExecuteAlways]
    public class PerRendererTexture : MonoBehaviour
    {
        [SerializeField]
        ShaderProperties _materialProperties;
        ShaderProperties MaterialProperties => _materialProperties;

        [SerializeField]
        string _textureName = "_MainTex";
        string TextureName => _textureName;

        [SerializeField]
        Texture _customTexture;
        Texture CustomTexture
        {
            get => _customTexture;
            set => _customTexture = value;
        }

        void SetTexture()
        {
            if(!MaterialProperties) return;
            if(!CustomTexture) return;

            Renderer renderer = GetComponent<Renderer>();
            if(!renderer) return;

            MaterialPropertyBlock mpb = ShaderProperties.MPB;

            MaterialProperties.SetProperties(mpb, renderer.sharedMaterial);

            mpb.SetTexture(TextureName, CustomTexture);

            renderer.SetPropertyBlock(mpb);
        }

        void Clear()
        {
            Renderer renderer = GetComponent<Renderer>();
            if(!renderer) return;

            renderer.SetPropertyBlock(null);
        }

        void OnEnable()
        {
            SetTexture();
        }

        void OnDisable()
        {
            Clear();
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(PerRendererTexture), true)]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                serializedObject.Update();

                SerializedProperty materialProperties = serializedObject.FindProperty(nameof(_materialProperties));
                EditorGUILayout.PropertyField(materialProperties);

                SerializedProperty textureName = serializedObject.FindProperty(nameof(_textureName));
                ShaderProperties shaderProperties = materialProperties.objectReferenceValue as ShaderProperties;
                if(shaderProperties)
                {
                    textureName.stringValue = shaderProperties.DrawPropertyMenu(ShaderProperties.PropertyTypes.Texture,
                                                                                textureName.stringValue,
                                                                                "Texture Property");
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_customTexture)));

                serializedObject.ApplyModifiedProperties();

                bool changed = EditorGUI.EndChangeCheck();

                if(changed)
                {
                    foreach(PerRendererTexture prt in targets)
                    {
                        prt.SetTexture();
                    }
                }
            }
        }
#endif
    }
}