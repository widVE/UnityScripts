using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Graphics
{
	[ExecuteAlways]
	public class LerpMaterial : MonoBehaviour, IInterpolatable
	{
		[SerializeField]
		[Tooltip("Material used at time 0.")]
		Material _materialA;
		Material MaterialA => _materialA;

		[SerializeField]
		[Tooltip("Material used at time 1.")]
		Material _materialB;
		Material MaterialB => _materialB;

		[SerializeField]
		[Tooltip("ShaderProperties object that uses the same shader as Materials A and B.")]
		ShaderProperties _shaderProperties;
		ShaderProperties ShaderProperties => _shaderProperties;

		enum RendererModes { Parent, Children }
		[SerializeField]
		[Tooltip("Specifies which Renderers are affected by this object.")]
		RendererModes _rendererMode = RendererModes.Parent;
		RendererModes RendererMode => _rendererMode;

		public bool Enabled => enabled;
		public bool FunctionWhenDisabled => false;

		[SerializeField]
		[HideInInspector]
		float _currentValue;
		float CurrentValue
		{
			get => _currentValue;
			set => _currentValue = value;
		}

		Material CurrentMaterial = null;

		Renderer[] _renderers;
		Renderer[] Renderers => _renderers ?? (_renderers = GetRenderers());

		static MaterialPropertyBlock _mpb;
		/// <summary>
		/// Reusable MaterialPropertyBlock.
		/// </summary>
		static MaterialPropertyBlock MPB => _mpb ?? (_mpb = new MaterialPropertyBlock());

		Renderer[] GetRenderers()
		{
			Renderer[] renderers = new Renderer[0];
			switch(RendererMode)
			{
				default:
				case RendererModes.Children:
					renderers = GetComponentsInChildren<Renderer>(true);
					break;
				case RendererModes.Parent:
					Renderer r = GetComponentInParent<Renderer>();
					if(r != null)
					{
						renderers = new Renderer[1];
						renderers[0] = r;
					}
					break;
			}
			return renderers;
		}

		void SetMaterial(Material m)
		{
			for(int i = 0; i < Renderers.Length; i++)
			{
				Renderers[i].sharedMaterial = m;
				ShaderProperties.SetRenderModes(m, Renderers[i].sharedMaterial);
			}
			CurrentMaterial = m;
		}

		/// <summary>
		/// Lerp materials on all child renderers between material A and material B.
		/// </summary>
		/// <param name="t">Value between 0 and 1.</param>
		public void Lerp(float t, bool trackValue=true)
		{
			if(!MaterialA) return;
			if(!MaterialB) return;
			if(!ShaderProperties) return;

			//set shared material on all renderers if necessary
			Material targetMaterial = Mathf.Approximately(t, 0) ? MaterialA : MaterialB; //should make a smarter check...
			if(CurrentMaterial != targetMaterial) SetMaterial(targetMaterial);

			//lerp renderers
			for(int i = 0; i < Renderers.Length; i++)
			{	//for each renderer:
				Renderer r = Renderers[i];
				//initialize property block with r's current properties
				//without this step, lerped values won't be set correctly
				ShaderProperties.SetPropertyBlock(MPB, r.sharedMaterial);
				//set lerped properties on property block
				ShaderProperties.LerpPropertyBlock(MPB, MaterialA, MaterialB, t);
				//apply properties to r
				r.SetPropertyBlock(MPB);
			}

			//store value for later
			if(trackValue) CurrentValue = t;
		}

		public void SetValue(float value)
		{
			Lerp(value);
		}

		void OnEnable()
		{   //refresh renderers
			_renderers = GetRenderers();
			//refresh lerp
			Lerp(CurrentValue);
		}

		void OnDisable()
		{   //remove any property block overrides by lerping to either 0 or 1
			if(CurrentMaterial == MaterialA) Lerp(0, false);
			else Lerp(1, false);
		}
	}
}