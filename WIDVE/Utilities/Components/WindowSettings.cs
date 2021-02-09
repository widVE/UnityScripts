using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class WindowSettings : MonoBehaviour
    {
		public enum StartModes { Windowed, Fullscreen }

		public enum ButtonModes { Toggle, GoFullscreen, GoWindowed }

		[SerializeField]
		StartModes StartMode = StartModes.Fullscreen;

        [SerializeField]
        Vector2Int DefaultWindowResolution = new Vector2Int(1280, 720);

		Vector2Int LastWindowResolution = Vector2Int.zero;

		[SerializeField]
		public ButtonModes ButtonMode = ButtonModes.Toggle;

		[SerializeField]
		ButtonFloat Button;

		public void ToggleWindowedMode()
		{
			if(Screen.fullScreen) SetWindowed();
			else SetFullscreen();
		}

        public void SetFullscreen()
		{
			LastWindowResolution = new Vector2Int(Screen.width, Screen.height);
            Resolution resolution = Screen.currentResolution;
            Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen);
		}

		public void SetWindowed()
		{
			if(LastWindowResolution == Vector2Int.zero) LastWindowResolution = DefaultWindowResolution;
			SetWindowed(LastWindowResolution);
		}

        public void SetWindowed(Vector2Int resolution)
		{
			Screen.SetResolution(resolution.x, resolution.y, FullScreenMode.Windowed);
		}

		public void ActivateButton(ButtonModes mode)
		{
			switch(mode)
			{
				default:
				case ButtonModes.Toggle:
					ToggleWindowedMode();
					break;
				case ButtonModes.GoFullscreen:
					SetFullscreen();
					break;
				case ButtonModes.GoWindowed:
					SetWindowed();
					break;
			}
		}

		public void SetStartMode(StartModes mode)
		{
			switch(mode)
			{
				case StartModes.Windowed:
					SetWindowed();
					break;
				default:
				case StartModes.Fullscreen:
					SetFullscreen();
					break;
			}
		}

		void Awake()
		{
			SetStartMode(StartMode);
		}

		void Update()
		{
			if(Button.GetUp())
			{
				ActivateButton(ButtonMode);
			}
		}
	}
}