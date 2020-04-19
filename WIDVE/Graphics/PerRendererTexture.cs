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

        static MaterialPropertyBlock _mpb;
        public static MaterialPropertyBlock MPB => _mpb ?? (_mpb = new MaterialPropertyBlock());

        void SetTexture()
        {
            if(!MaterialProperties) return;
            if(!CustomTexture) return;

            Renderer renderer = GetComponent<Renderer>();
            if(!renderer) return;

            MaterialProperties.SetProperties(MPB, renderer.sharedMaterial);

            MPB.SetTexture(TextureName, CustomTexture);

            renderer.SetPropertyBlock(MPB);
        }

        void ClearTexture()
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
            ClearTexture();
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(PerRendererTexture), true)]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

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