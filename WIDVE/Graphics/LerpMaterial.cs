using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Graphics
{
	[ExecuteAlways]
	public class LerpMaterial : RendererController
	{
		[SerializeField]
		[Tooltip("Material used at time 0.")]
		Material _materialA;
		Material MaterialA => _materialA;

		[SerializeField]
		[Tooltip("Material used at time 1.")]
		Material _materialB;
		Material MaterialB => _materialB;

		Material CurrentMaterial = null;

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
			{
				//for each renderer:
				Renderer r = Renderers[i];

				//initialize property block with r's current properties
				//without this step, lerped values won't be set correctly
				ShaderProperties.SetProperties(MPB, r.sharedMaterial);

				//set lerped properties on property block
				ShaderProperties.LerpProperties(MPB, MaterialA, MaterialB, t);

				//apply properties to r
				r.SetPropertyBlock(MPB);
			}

			//store value for later
			if(trackValue) CurrentValue = t;
		}

		public override void SetValue(float value)
		{
			Lerp(value);
		}

		void OnEnable()
		{
			//refresh lerp
			Lerp(CurrentValue);
		}

		void OnDisable()
		{   
			//remove any property block overrides by lerping to either 0 or 1
			if(CurrentMaterial == MaterialA) Lerp(0, false);
			else Lerp(1, false);
		}
	}
}