using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public class QuitApplication : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The application will quit when this button is pressed.")]
		ButtonFloat _quitButton;
		ButtonFloat QuitButton => _quitButton;

		public void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		void Update()
		{
			if(QuitButton && QuitButton.GetUp()) Quit();
		}
	}
}