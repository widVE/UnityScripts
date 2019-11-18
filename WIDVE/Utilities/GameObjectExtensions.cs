using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace WIDVE.Utilities.Extensions
{
	public static class GameObjectExtensions
	{
		/// <summary>
		/// Returns true if this GameObject exists in a Scene.
		/// <para>Use this to distinguish between e.g. regular GameObjects and Prefab Asset GameObjects.</para>
		/// </summary>
		public static bool ExistsInScene(this GameObject gameObject)
		{
			bool inScene;
			GameObjectEnvironments environment = gameObject.GetGameObjectEnvironment();
			switch (environment)
			{
				default:
				case GameObjectEnvironments.Scene:
				case GameObjectEnvironments.PrefabInstance:
					inScene = true;
					break;
				case GameObjectEnvironments.PrefabAsset:
				case GameObjectEnvironments.PrefabStage:
				case GameObjectEnvironments.NestedPrefabStage:
				case GameObjectEnvironments.PrefabImport:
				case GameObjectEnvironments.PreviewScene:
					inScene = false;
					break;
			}
			return inScene;
		}

		/// <summary>
		/// Environments that GameObjects can exist in.
		/// </summary>
		public enum GameObjectEnvironments { Unknown, PrefabAsset, PrefabStage, NestedPrefabStage, PrefabInstance, Scene, PrefabImport, PreviewScene }

		/// <summary>
		/// Return the environment this GameObject is contained in.
		/// </summary>
		public static GameObjectEnvironments GetGameObjectEnvironment(this GameObject gameObject)
		{   //https://github.com/Unity-Technologies/PrefabAPIExamples/blob/master/Assets/Scripts/GameObjectTypeLogging.cs
			GameObjectEnvironments environment;
#if UNITY_EDITOR
			if (EditorUtility.IsPersistent(gameObject))
			{	//game object exists on disk
				if (!PrefabUtility.IsPartOfPrefabAsset(gameObject))
				{   //The GameObject is a temporary object created during import.
					//OnValidate() is called two times with a temporary object during import:
					//	First time is when saving cloned objects to .prefab file.
					//	Second event is when reading .prefab file objects during import
					environment = GameObjectEnvironments.PrefabImport;
				}
				else
				{   //GameObject is part of an imported Prefab Asset (from the Library folder)
					environment = GameObjectEnvironments.PrefabAsset;
				}
			}
			else
			{	//If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
				StageHandle mainStage = StageUtility.GetMainStageHandle();
				StageHandle currentStage = StageUtility.GetStageHandle(gameObject);
				if (currentStage == mainStage)
				{	//viewing scenes in the main stage (aka -not- editing in prefab mode)
					if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
					{	//GameObject is part of a Prefab Instance in the MainStage
						environment = GameObjectEnvironments.PrefabInstance;
					}
					else
					{	//GameObject is a plain GameObject in the MainStage
						environment = GameObjectEnvironments.Scene;
					}
				}
				else
				{	//editing a prefab in prefab mode
					PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
					if (prefabStage != null)
					{
						if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
						{	//GameObject is in a PrefabStage and is nested.
							environment = GameObjectEnvironments.NestedPrefabStage;
						}
						else
						{	//GameObject is in a PrefabStage.
							environment = GameObjectEnvironments.PrefabStage;
						}
					}
					else if (EditorSceneManager.IsPreviewSceneObject(gameObject))
					{   //GameObject is not in the MainStage, nor in a PrefabStage.
						//But it is in a PreviewScene so could be used for Preview rendering or other utilities.
						environment = GameObjectEnvironments.PreviewScene;
					}
					else
					{   //Unknown GameObject Info
						environment = GameObjectEnvironments.Unknown;
					}
				}
			}
#else
			//Can't really check outside editor...
			environment = GameObjectEnvironments.Unknown;
#endif
			return environment;
		}

		/// <summary>
		/// Returns true if this GameObject or any of its parent objects are currently selected in the Editor.
		/// <para>Returns false when not in Edit mode.</para>
		/// </summary>
		public static bool IsSelected(this GameObject gameObject)
		{
			bool isSelected = false;
#if UNITY_EDITOR
			Object[] selectedObjects = UnityEditor.Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep);
			for(int i = 0; i < selectedObjects.Length; i++)
			{
				if(selectedObjects[i] as GameObject == gameObject)
				{
					isSelected = true;
					break;
				}
			}
#endif
			return isSelected;
		}

		/// <summary>
		/// Returns true if this GameObject is part of the current top-level selection in Editor.
		/// <para>Returns false outside Editor.</para>
		/// </summary>
		public static bool IsTopLevelSelection(this GameObject gameObject)
		{
			bool isTopLevelSelection = false;
#if UNITY_EDITOR
			Object[] topLevelObjects = UnityEditor.Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel);
			for(int i = 0; i < topLevelObjects.Length; i++)
			{
				if(topLevelObjects[i] as GameObject == gameObject)
				{
					isTopLevelSelection = true;
					break;
				}
			}
#endif
			return isTopLevelSelection;
		}

		/// <summary>
		/// In Edit mode, mark the scene containing this GameObject as dirty.
		/// <para>In Play mode, does nothing.</para>
		/// </summary>
		public static void MarkDirty(this GameObject gameObject, bool force=false)
		{
#if UNITY_EDITOR
			if (Application.isPlaying) return;
			if (!force && !gameObject.ExistsInScene()) return;
			EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
		}

		/// <summary>
		/// Instantiate a prefab as a child of this GameObject.
		/// </summary>
		/// <param name="keepPrefabConnection">When true, keep prefab connection in edit mode.</param>
		public static Object InstantiatePrefab(this GameObject gameObject, Object prefab, bool keepPrefabConnection = true, bool keepPrefabTransform = false)
		{
			Object instance = null;
			if (!keepPrefabConnection || Application.isPlaying)
			{
				instance = Object.Instantiate(original: prefab, parent: gameObject.transform);			
			}
			else
			{
#if UNITY_EDITOR
				instance = PrefabUtility.InstantiatePrefab(assetComponentOrGameObject: prefab, parent: gameObject.transform);
#endif
			}
			if (keepPrefabTransform && instance != null)
			{	//keep same world position, rotation, and scale as original prefab
				Transform prefabTransform = (prefab as GameObject).transform;
				Transform instanceTransform = (instance as GameObject).transform;
				instanceTransform.SetWorldScale(prefabTransform.lossyScale);
				instanceTransform.SetPositionAndRotation(prefabTransform.position, prefabTransform.rotation);
			}
			return instance;
		}
	}
}