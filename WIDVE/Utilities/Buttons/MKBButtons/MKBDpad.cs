using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    [CreateAssetMenu(fileName = nameof(MKBDpad), menuName = nameof(Button) + "/" + nameof(MKBDpad), order = B_ORDER)]
    public class MKBDpad : ButtonVector2
    {
        [SerializeField]
        ButtonFloat _up;
        ButtonFloat Up => _up;

        [SerializeField]
        ButtonFloat _down;
        ButtonFloat Down => _down;

        [SerializeField]
        ButtonFloat _left;
        ButtonFloat Left => _left;

        [SerializeField]
        ButtonFloat _right;
        ButtonFloat Right => _right;

        public override Vector2 GetRawValue()
        {
            if(!Up || !Down || !Left || !Right) return Vector2.zero;

            Vector2 value;

            value.x = Mathf.Clamp(Right.GetValue() - Left.GetValue(), -1, 1);
            value.y = Mathf.Clamp(Up.GetValue() - Down.GetValue(), -1, 1);

            return value;
        }
    }
}