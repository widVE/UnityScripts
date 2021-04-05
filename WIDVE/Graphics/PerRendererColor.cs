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
        public ShaderProperties Properties
        {
            get => _properties;
            set => _properties = value;
        }

        [SerializeField]
        Color _color = Color.white;
        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        [SerializeField]
        string _colorName = "_BaseColor";
        public string ColorName
        {
            get => _colorName;
            set => _colorName = value;
        }

        public void SetColor()
        {
            if(!Properties) return;

            Renderer renderer = GetComponentInChildren<Renderer>();
            if(!renderer) return;

            SetColor(renderer, Properties, ColorName, Color);
        }

        public static void SetColor(Renderer renderer, ShaderProperties shaderProperties, string colorName, Color color)
        {
            if(!renderer) return;
            if(!shaderProperties) return;

            MaterialPropertyBlock mpb = ShaderProperties.GetMPB();

            shaderProperties.SetProperties(mpb, renderer.sharedMaterial);

            mpb.SetColor(colorName, color);

            renderer.SetPropertyBlock(mpb);
        }

        public void Clear()
        {
            Renderer renderer = GetComponent<Renderer>();
            if(!renderer) return;

            renderer.SetPropertyBlock(null);
        }

        void OnEnable()
        {
            if(enabled) SetColor();
        }

        void Start()
        {
            if(enabled) SetColor();
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
                        if(prc.enabled)
                        {
                            prc.SetColor();
                        }
                    }
                }
            }
        }
#endif
    }
}