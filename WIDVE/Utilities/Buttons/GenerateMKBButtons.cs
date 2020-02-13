using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
	static class GenerateMKBButtons
	{
#if UNITY_EDITOR
		static string ButtonsFolder = "WIDVE/MKBButtons";

		static Dictionary<KeyCode, string> DefaultButtons = new Dictionary<KeyCode, string>
		{
			//the usual suspects
			{ KeyCode.Space, nameof(KeyCode.Space) },
			{ KeyCode.Return, "Enter"},
			{ KeyCode.Escape, nameof(KeyCode.Escape) },
			{ KeyCode.Tab, nameof(KeyCode.Tab) },

			//modifiers
			{ KeyCode.LeftShift, nameof(KeyCode.LeftShift) },
			{ KeyCode.LeftControl, nameof(KeyCode.LeftControl) },
			{ KeyCode.LeftAlt, nameof(KeyCode.LeftAlt) },

			//arrow keys
			{ KeyCode.UpArrow, "Up" },
			{ KeyCode.DownArrow, "Down" },
			{ KeyCode.LeftArrow, "Left" },
			{ KeyCode.RightArrow, "Right" },

			//mouse buttons
			{ KeyCode.Mouse0, "Left Click" },
			{ KeyCode.Mouse1, "Right Click" },
			{ KeyCode.Mouse2, "Middle Click" }
		};

		[MenuItem("WIDVE/Generate Mouse and Keyboard Buttons")]
		static void Generate()
		{
			int buttonsCreated = 0;

			foreach(KeyValuePair<KeyCode, string> defaultButton in DefaultButtons)
			{
				//create a new ScriptableObject for this button
				MKBButton button = new MKBButton(defaultButton.Key);
				button.name = defaultButton.Value;

				//save the button as an asset
				ScriptableObjectAssetCreator.SaveToAsset(button, ButtonsFolder);

				buttonsCreated++;
			}

			//save and refresh the AssetDatabase when done creating all buttons
			ScriptableObjectAssetCreator.SaveAndRefresh();

			Debug.Log($"Created {buttonsCreated} MKBButtons in {ButtonsFolder}");
		}
#endif
	}
}