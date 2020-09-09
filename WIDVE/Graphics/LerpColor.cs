using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics
{
    public class LerpColor : RendererController
    {
        [SerializeField]
        Color _colorA = Color.white;
        Color ColorA => _colorA;

        [SerializeField]
        Color _colorB = Color.white;
        Color ColorB => _colorB;

        [SerializeField]
        string _colorName = "_BaseColor";
        public string ColorName
        {
            get => _colorName;
            set => _colorName = value;
        }

        public void Lerp(float t, bool trackValue = true)
        {
            Color currentColor = Color.Lerp(ColorA, ColorB, t);

            for(int i = 0; i < Renderers.Length; i++)
            {
                Renderer r = Renderers[i];

                PerRendererColor.SetColor(r, ShaderProperties, ColorName, currentColor);
            }

            if(trackValue) CurrentValue = t;
        }

        public override void SetValue(float value)
        {
            Lerp(value);
        }

        protected void OnEnable()
        {
            Lerp(CurrentValue);
        }

        protected void OnDisable()
        {
            Lerp(0, false);
            RemoveMaterialPropertyBlock();
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(LerpColor), true)]
        new class Editor : RendererController.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawRCInspector();

                EditorGUI.BeginChangeCheck();

                serializedObject.Update();

                SerializedProperty colorName = serializedObject.FindProperty(nameof(_colorName));
                SerializedProperty properties = serializedObject.FindProperty(nameof(_shaderProperties));
                ShaderProperties shaderProperties = properties.objectReferenceValue as ShaderProperties;
                if(shaderProperties)
                {
                    colorName.stringValue = shaderProperties.DrawPropertyMenu(ShaderProperties.PropertyTypes.Color,
                                                                                colorName.stringValue,
                                                                                "Color Property");
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_colorA)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_colorB)));

                serializedObject.ApplyModifiedProperties();

                bool changed = EditorGUI.EndChangeCheck();

                if(changed)
                {
                    foreach(LerpColor lc in targets)
                    {
                        lc.SetValue(lc.CurrentValue);
                    }
                }
            }
        }
#endif
    }
}