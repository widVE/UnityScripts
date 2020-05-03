using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics
{
    [ExecuteAlways]
    public class PerRendererColor : MonoBehaviour
    {
        [SerializeField]
        ShaderProperties _properties;
        ShaderProperties Properties => _properties;

        [SerializeField]
        Color _color;
        Color Color => _color;

        [SerializeField]
        string _colorName = "_Color";
        string ColorName => _colorName;

        void SetColor()
        {
            if(!Properties) return;

            Renderer renderer = GetComponent<Renderer>();
            if(!renderer) return;

            MaterialPropertyBlock mpb = ShaderProperties.MPB;

            Properties.SetProperties(mpb, renderer.sharedMaterial);

            mpb.SetColor(ColorName, Color);

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
            SetColor();
        }

        void OnDisable()
        {
            Clear();
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(PerRendererColor), true)]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                serializedObject.Update();

                SerializedProperty properties = serializedObject.FindProperty(nameof(_properties));
                EditorGUILayout.PropertyField(properties);

                SerializedProperty colorName = serializedObject.FindProperty(nameof(_colorName));
                ShaderProperties shaderProperties = properties.objectReferenceValue as ShaderProperties;
                if(shaderProperties)
                {
                    colorName.stringValue = shaderProperties.DrawPropertyMenu(ShaderProperties.PropertyTypes.Color,
                                                                                colorName.stringValue,
                                                                                "Color Property");
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_color)));

                serializedObject.ApplyModifiedProperties();

                bool changed = EditorGUI.EndChangeCheck();

                if(changed)
                {
                    foreach(PerRendererColor prc in targets)
                    {
                        prc.SetColor();
                    }
                }
            }
        }
#endif
    }
}