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

        public void Lerp(float t, bool trackValue = true)
        {
            for(int i = 0; i < Components.Length; i++)
            {
                UnityEngine.UI.Graphic g = Components[i];
                g.color = Color.Lerp(ColorA, ColorB, t);
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