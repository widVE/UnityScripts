using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics
{
	[ExecuteAlways]
	public class Fader : RendererController
	{
		enum Modes { SingleMaterial, SeparateMaterials }

		[SerializeField]
		[HideInInspector]
		Modes _fadeMode = Modes.SingleMaterial;
		Modes FadeMode => _fadeMode;

		[SerializeField]
		[HideInInspector]
		Material _opaqueMaterial;
		Material OpaqueMaterial => _opaqueMaterial;

		[SerializeField]
		[HideInInspector]
		Material _transparentMaterial;
		Material TransparentMaterial => _transparentMaterial;

		Material CurrentMaterial;

		public void SetAlpha(float alpha, bool trackValue=true)
		{
			if(!ShaderProperties) return;

			if(FadeMode == Modes.SeparateMaterials)
			{
				if(!OpaqueMaterial || !TransparentMaterial) return;

				//set opaque or transparent material
				Material m = Mathf.Approximately(alpha, 1) ? OpaqueMaterial : TransparentMaterial;
				if(CurrentMaterial != m)
				{
					SetMaterial(m);
					CurrentMaterial = m;
				}
			}

			Dictionary<string, Color> initialColors = new Dictionary<string, Color>();

			for(int i = 0; i < Renderers.Length; i++)
			{
				//for each renderer:
				Renderer r = Renderers[i];

				//just skip null/missing renderers for now - not sure how to keep the list up to date in every situation yet
				if(!r) continue;

				//turn renderer on or off
				if(Mathf.Approximately(alpha, 0f))
				{
					if(r.enabled) r.enabled = false;
				}
				else if(!r.enabled) r.enabled = true;

				//initialize property block with default material values
				ShaderProperties.SetProperties(MPB, r.sharedMaterial, clear: true);

				//add per-renderer colors back in
				PerRendererColor prc = r.GetComponent<PerRendererColor>();
				if(prc) MPB.SetColor(prc.ColorName, prc.Color);

				//set alpha value of all colors
				ShaderProperties.SetAlpha(MPB, alpha);

				//apply property block
				r.SetPropertyBlock(MPB);
			}

			if(trackValue) CurrentValue = alpha;
		}

		public override void SetValue(float value)
		{
			SetAlpha(value);
		}

		void OnEnable()
		{
			//refresh fading
			SetAlpha(CurrentValue);
		}

		void OnDisable()
		{
			//remove any fading
			SetAlpha(1, false);
			RemoveMaterialPropertyBlock();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(Fader), true)]
		new class Editor : RendererController.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				serializedObject.Update();

				SerializedProperty fadeMode = serializedObject.FindProperty(nameof(_fadeMode));
				EditorGUILayout.PropertyField(fadeMode);

				if(fadeMode.enumValueIndex == (int)Modes.SeparateMaterials)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_opaqueMaterial)));
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_transparentMaterial)));
				}

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}