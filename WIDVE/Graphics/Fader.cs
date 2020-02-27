using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Graphics
{
	[ExecuteAlways]
	public class Fader : RendererController
	{	
		public void SetAlpha(float alpha, bool trackValue=true)
		{
			if(!ShaderProperties) return;

			for(int i = 0; i < Renderers.Length; i++)
			{
				//for each renderer:
				Renderer r = Renderers[i];

				//initialize property block
				ShaderProperties.SetProperties(MPB, r.sharedMaterial);

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

		void Awake()
		{
			//make sure objects start fully faded in...
			CurrentValue = 1;
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
		}
	}
}