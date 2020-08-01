using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Graphics
{
    public class LerpGraphic : GraphicController
    {
        [SerializeField]
        Color _colorA = Color.white;
        Color ColorA => _colorA;

        [SerializeField]
        Color _colorB = Color.white;
        Color ColorB => _colorB;

        [SerializeField]
        [Tooltip("If ColorName is given, sets the named color on the material used by each Graphic.")]
        string _colorName = "";
        public string ColorName
		{
            get => _colorName;
            set => _colorName = value;
		}

        public void Lerp(float t, bool trackValue = true)
        {
            Color lerpedColor = Color.Lerp(ColorA, ColorB, t);

            for(int i = 0; i < Components.Length; i++)
            {
                UnityEngine.UI.Graphic g = Components[i];
                g.color = lerpedColor;

                //also set color by name, for when setting the Graphic's color has no effect...
                if(!string.IsNullOrEmpty(ColorName))
                {
                    g.material.SetColor(ColorName, lerpedColor);
                }
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
        }
    }
}