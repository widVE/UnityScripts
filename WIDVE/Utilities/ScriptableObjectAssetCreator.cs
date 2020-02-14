using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
	/// <summary>
	/// [Editor only] Creates ScriptableObject assets.
	/// </summary>
	public static class ScriptableObjectAssetCreator
	{
#if UNITY_EDITOR
		public const string ASSETS_FOLDER = "Assets";

		public const string EXTENSION = ".asset";

		/// <summary>
		/// Creates and returns a new ScriptableObject as an asset.
		/// </summary>
		/// <typeparam name="T">Type of ScriptableObject to create.</typeparam>
		/// <param name="folder">Folder to save to.</param>
		/// <param name="filename">Filename of the saved asset.</param>
		/// <param name="saveAndRefresh">Optional: save and refresh the AssetDatabase after asset creation.</param>
		/// <param name="focus">Optional: focus on the newly created asset in the Editor when done.</param>
		/// <returns>The newly created ScriptableObject.</returns>
		public static T CreateAsset<T>(string folder, string filename = null, bool saveAndRefresh = true, bool focus = false) where T : ScriptableObject
		{   //https://wiki.unity3d.com/index.php/CreateScriptableObjectAsset
			//create a new ScriptableObject instance
			T asset = ScriptableObject.CreateInstance<T>();

			//save this instance to an asset and return it
			SaveToAsset(asset, folder, filename, saveAndRefresh, focus);
			return asset;
		}

		/// <summary>
		/// Saves the given ScriptableObject instance to an asset.
		/// </summary>
		/// <param name="scriptableObject">ScriptableObject to save.</param>
		/// <param name="folder">Folder to save to.</param>
		/// <param name="filename">Filename of the saved asset.</param>
		/// <param name="saveAndRefresh">Optional: save and refresh the AssetDatabase after asset creation.</param>
		/// <param name="focus">Optional: focus on the newly created asset in the Editor when done.</param>
		/// <returns>Path to the created asset.</returns>
		public static string SaveToAsset(ScriptableObject scriptableObject, string folder, string filename = null, bool saveAndRefresh = true, bool focus = false)
		{
			//generate the filepath for the saved asset
			if(string.IsNullOrEmpty(folder)) folder = ASSETS_FOLDER;
			if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
			if(string.IsNullOrEmpty(filename)) filename = scriptableObject.name;
			if(!filename.EndsWith(EXTENSION)) filename += EXTENSION;
			string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, filename));
			
			//create the asset
			AssetDatabase.CreateAsset(scriptableObject, assetPath);

			//optional: save and refresh after creating the asset
			if(saveAndRefresh)
			{
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				//optional: after saving, focus on the newly created asset
				if(focus)
				{
					EditorUtility.FocusProjectWindow();
					Selection.activeObject = scriptableObject;
				}
			}

			//return the path to the newly created asset
			return AssetDatabase.GetAssetPath(scriptableObject);
		}
#endif
	}
}