using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
	static class GenerateOculusButtons
	{
#if UNITY_EDITOR
		const string BUTTONS_FOLDER = WIDVEEditor.FOLDER + "/OculusButtons";

		static Dictionary<OVRInput.RawButton, string> DefaultButtons = new Dictionary<OVRInput.RawButton, string>
		{
			{ OVRInput.RawButton.A , nameof(OVRInput.RawButton.A) },
			{ OVRInput.RawButton.B , nameof(OVRInput.RawButton.B) },
			{ OVRInput.RawButton.X , nameof(OVRInput.RawButton.X) },
			{ OVRInput.RawButton.Y , nameof(OVRInput.RawButton.Y) },
			{ OVRInput.RawButton.Start , "Menu" },
			{ OVRInput.RawButton.Back , nameof(OVRInput.RawButton.Back) },
			{ OVRInput.RawButton.LThumbstick , "LeftThumbstickClick" },
			{ OVRInput.RawButton.RThumbstick , "RightThumbstickClick" }
		};

		static Dictionary<OVRInput.RawAxis1D, string> DefaultAxes1D = new Dictionary<OVRInput.RawAxis1D, string>
		{
			{ OVRInput.RawAxis1D.LHandTrigger, "LeftGrip" },
			{ OVRInput.RawAxis1D.RHandTrigger, "RightGrip" },
			{ OVRInput.RawAxis1D.LIndexTrigger, "LeftTrigger" },
			{ OVRInput.RawAxis1D.RIndexTrigger, "RightTrigger" }
		};

		static Dictionary<OVRInput.RawAxis2D, string> DefaultAxes2D = new Dictionary<OVRInput.RawAxis2D, string>
		{
			{ OVRInput.RawAxis2D.LThumbstick, "LeftThumbstick" },
			{ OVRInput.RawAxis2D.RThumbstick, "RightThumbstick" },
			{ OVRInput.RawAxis2D.RTouchpad, "Touchpad" }
		};

		[MenuItem(WIDVEEditor.MENU + "/Generate Oculus Buttons")]
		static void Generate()
		{
			int buttonsCreated = 0;

			List<Button> allButtons = new List<Button>();

			//create scriptable objects for each default button and axis
			foreach(KeyValuePair<OVRInput.RawButton, string> rawButton in DefaultButtons)
			{
				OculusButton button = ScriptableObject.CreateInstance<OculusButton>();
				button.OVRButton = rawButton.Key;
				button.name = rawButton.Value;
				allButtons.Add(button);
			}

			foreach(KeyValuePair<OVRInput.RawAxis1D, string> rawAxis1D in DefaultAxes1D)
			{
				OculusAxis1D axis1D = ScriptableObject.CreateInstance<OculusAxis1D>();
				axis1D.OVRAxis = rawAxis1D.Key;
				axis1D.name = rawAxis1D.Value;
				allButtons.Add(axis1D);
			}

			foreach(KeyValuePair<OVRInput.RawAxis2D, string> rawAxis2D in DefaultAxes2D)
			{
				OculusAxis2D axis2D = ScriptableObject.CreateInstance<OculusAxis2D>();
				axis2D.OVRAxis = rawAxis2D.Key;
				axis2D.name = rawAxis2D.Value;
				allButtons.Add(axis2D);
			}

			//save scriptable objects to assets
			foreach(Button button in allButtons)
			{
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

			Debug.Log($"Created {buttonsCreated} OculusButtons in {BUTTONS_FOLDER}");
		}
#endif
	}
}