using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class WindowSettings : MonoBehaviour
    {
        [SerializeField]
        bool StartWindowed = false;

        [SerializeField]
        Vector2Int WindowResolution = new Vector2Int(1280, 720);

		[SerializeField]
		ButtonFloat ToggleButton;

		public void ToggleWindowedMode()
		{
			if(Screen.fullScreen) SetWindowed();
			else SetFullscreen();
		}

        public void SetFullscreen()
		{
            Resolution resolution = Screen.currentResolution;
            Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen);
		}

		public void SetWindowed()
		{
			SetWindowed(WindowResolution);
		}

        public void SetWindowed(Vector2Int resolution)
		{
			Screen.SetResolution(resolution.x, resolution.y, FullScreenMode.Windowed);
		}

		void Awake()
		{
			if(StartWindowed) SetWindowed();
		}

		void Update()
		{
			if(ToggleButton.GetUp())
			{
				ToggleWindowedMode();
			}
		}
	}
}