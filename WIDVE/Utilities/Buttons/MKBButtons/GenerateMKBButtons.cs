using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
	static class GenerateMKBButtons
	{
#if UNITY_EDITOR
		const string BUTTONS_FOLDER = WIDVEEditor.FOLDER + "/MKBButtons";

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

		[MenuItem(WIDVEEditor.MENU + "/Generate Mouse and Keyboard Buttons")]
		static void Generate()
		{
			int buttonsCreated = 0;

			foreach(KeyValuePair<KeyCode, string> defaultButton in DefaultButtons)
			{
				//create a new ScriptableObject for this button
				MKBButton button = ScriptableObject.CreateInstance<MKBButton>();
				button.Key = defaultButton.Key;
				button.name = defaultButton.Value;

				//check that the button does not already exist
				string buttonPath = Path.Combine(BUTTONS_FOLDER, button.name + ScriptableObjectAssetCreator.EXTENSION);
				if(AssetDatabase.GetMainAssetTypeAtPath(buttonPath) == null)
				{
					//save the button as an asset
					ScriptableObjectAssetCreator.SaveToAsset(button, BUTTONS_FOLDER, saveAndRefresh: false);
					buttonsCreated++;
				}
			}

			//save and refresh the AssetDatabase when done creating all buttons
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Debug.Log($"Created {buttonsCreated} MKBButtons in {BUTTONS_FOLDER}");
		}
#endif
	}
}