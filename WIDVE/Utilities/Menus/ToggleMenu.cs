using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class ToggleMenu : MonoBehaviour
    {
        [SerializeField]
        ButtonFloat _toggleButton;
        ButtonFloat ToggleButton => _toggleButton;

        [SerializeField]
        Menu _menu;
        Menu Menu => _menu;

        public void Toggle()
        {
            if(Menu.IsOpen) Menu.Close();
            else Menu.Open();
        }

        void Update()
        {
            if(!ToggleButton) return;

            if(ToggleButton.GetDown())
            {
                Toggle();
            }
        }
    }
}