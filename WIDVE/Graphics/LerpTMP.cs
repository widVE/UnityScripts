using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace WIDVE.Graphics
{
    public class LerpTMP : TMPController
    {
        [SerializeField]
        Color _colorA = Color.white;
        Color ColorA => _colorA;

        [SerializeField]
        Color _colorB = Color.white;
        Color ColorB => _colorB;

        public void Lerp(float t, bool trackValue = true)
        {
            Color lerpedColor = Color.Lerp(ColorA, ColorB, t);

            for(int i = 0; i < Components.Length; i++)
            {
                TMP_Text text = Components[i];
                text.color = lerpedColor;
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