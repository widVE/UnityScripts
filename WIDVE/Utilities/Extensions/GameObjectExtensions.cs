using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace WIDVE.Utilities
{
	public static class GameObjectExtensions
	{
		/// <summary>
		/// Environments that GameObjects can exist in.
		/// </summary>
		public enum GameObjectEnvironments { Unknown, PrefabAsset, PrefabStage, NestedPrefabStage, PrefabInstance, Scene, PrefabImport, PreviewScene }

		/// <summary>
		/// Returns the environment this GameObject exists in.
		/// </summary>
		public static GameObjectEnvironments GetGameObjectEnvironment(this GameObject gameObject)
		{
			//based on https://github.com/Unity-Technologies/PrefabAPIExamples/blob/master/Assets/Scripts/GameObjectTypeLogging.cs
			//most comments also from there

			GameObjectEnvironments environment;

#if UNITY_EDITOR
			//check if game object exists on disk
			if (EditorUtility.IsPersistent(gameObject))
			{
				if (!PrefabUtility.IsPartOfPrefabAsset(gameObject))
				{
					//The GameObject is a temporary object created during import.
					//OnValidate() is called two times with a temporary object during import:
					//	First time is when saving cloned objects to .prefab file.
					//	Second event is when reading .prefab file objects during import
					environment = GameObjectEnvironments.PrefabImport;
				}
				else
				{
					//GameObject is part of an imported Prefab Asset (from the Library folder)
					environment = GameObjectEnvironments.PrefabAsset;
				}
			}

			else
			{
				//If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
				StageHandle mainStage = StageUtility.GetMainStageHandle();
				StageHandle currentStage = StageUtility.GetStageHandle(gameObject);

				if (currentStage == mainStage)
				{
					//viewing scenes in the main stage (aka -not- editing in prefab mode)
					if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
					{
						//GameObject is part of a Prefab Instance in the MainStage
						environment = GameObjectEnvironments.PrefabInstance;
					}
					else
					{
						//GameObject is a plain GameObject in the MainStage
						environment = GameObjectEnvironments.Scene;
					}
				}

				else
				{
					//editing a prefab in prefab mode
					PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);

					if (prefabStage != null)
					{
						if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
						{
							//GameObject is in a PrefabStage and is nested.
							environment = GameObjectEnvironments.NestedPrefabStage;
						}
						else
						{
							//GameObject is in a PrefabStage.
							environment = GameObjectEnvironments.PrefabStage;
						}
					}

					else if (EditorSceneManager.IsPreviewSceneObject(gameObject))
					{
						//GameObject is not in the MainStage, nor in a PrefabStage.
						//But it is in a PreviewScene so could be used for Preview rendering or other utilities.
						environment = GameObjectEnvironments.PreviewScene;
					}

					else
					{
						//Unknown GameObject Info
						environment = GameObjectEnvironments.Unknown;
					}
				}
			}

#else
			//Can't do any of the above checks outside of the editor
			environment = GameObjectEnvironments.Unknown;
#endif

			return environment;
		}
		
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
		/// Returns true if this GameObject or any of its parent objects are currently selected in the Editor.
		/// <para>Returns false outside the Editor.</para>
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
		/// Returns true if this GameObject is part of the current top-level selection in the Editor.
		/// <para>Returns false outside the Editor.</para>
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
		/// In Edit mode, marks the scene containing this GameObject as dirty.
		/// <para>In Prefab mode, marks the prefab asset containing this GameObject as dirty.</para>
		/// <para>In Play mode, does nothing.</para>
		/// </summary>
		/// <returns>True if the object was marked dirty, false otherwise.</returns>
		public static bool MarkSceneDirty(this GameObject gameObject)
		{
#if UNITY_EDITOR
			//skip in play mode
			if (Application.isPlaying) return false;

			GameObjectEnvironments environment = gameObject.GetGameObjectEnvironment();
			switch(environment)
			{
				//if the GameObject is being edited as part of a Scene:
				case GameObjectEnvironments.Scene:
				case GameObjectEnvironments.PrefabInstance:
					//mark the scene containing this GameObject as dirty
					EditorSceneManager.MarkSceneDirty(gameObject.scene);
					return true;

				//if the GameObject is being edited as part of a Prefab:
				case GameObjectEnvironments.PrefabStage:
				case GameObjectEnvironments.NestedPrefabStage:
					//mark the Prefab containing this GameObject as dirty
					//do this by using the currently open PrefabStage scene
					PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
					if(prefabStage != null)
					{
						EditorSceneManager.MarkSceneDirty(prefabStage.scene);
						return true;
					}
					else return false;

				//do nothing if the GameObject is from some other environment
				case GameObjectEnvironments.PrefabAsset:
				case GameObjectEnvironments.PrefabImport:
				case GameObjectEnvironments.PreviewScene:
				default:
					return false;
			}
#else
			//do nothing outside editor
			return false;
#endif
		}

		/// <summary>
		/// Instantiates a new prefab instance as a child of this GameObject.
		/// </summary>
		/// <param name="keepPrefabConnection">When true, keeps prefab connection (only used in Edit mode).</param>
		public static Object InstantiatePrefab(this GameObject gameObject, Object prefab, bool keepPrefabConnection = true, bool keepPrefabTransform = false)
		{
			Object instance = null;

			if (!keepPrefabConnection || Application.isPlaying)
			{
				//instantiate the object without a prefab connection
				instance = Object.Instantiate(original: prefab, parent: gameObject.transform);			
			}

#if UNITY_EDITOR
			else
			{
				//instantiate the object and keep the prefab connection
				instance = PrefabUtility.InstantiatePrefab(assetComponentOrGameObject: prefab, parent: gameObject.transform);
			}
#endif

			//keep original name (no more appending '(Clone)')
			instance.name = prefab.name;

			if(keepPrefabTransform && instance != null)
			{
				//keep same world position, rotation, and scale as original prefab
				Transform prefabTransform = (prefab as GameObject).transform;
				Transform instanceTransform = (instance as GameObject).transform;
				instanceTransform.SetWorldScale(prefabTransform.lossyScale);
				instanceTransform.SetPositionAndRotation(prefabTransform.position, prefabTransform.rotation);
			}

			return instance;
		}

		public enum SearchModes { Parent, Parents, Self, Child, Children, Custom }

		public static T GetComponentInHierarchy<T>(this GameObject gameObject, SearchModes mode) where T : class
		{
			T t;

			switch(mode)
			{
				case SearchModes.Parent:
				case SearchModes.Parents:
					t = gameObject.GetComponentInParent<T>();
					break;

				case SearchModes.Self:
					t = gameObject.GetComponent<T>();
					break;

				case SearchModes.Child:
				case SearchModes.Children:
					t = gameObject.GetComponentInChildren<T>();
					break;

				default:
					t = null;
					break;
			}

			return t;
		}

		public static T[] GetComponentsInHierarchy<T>(this GameObject gameObject, SearchModes mode, List<GameObject> customObjects = null) where T : class
		{
			T[] components;

			switch(mode)
			{
				case SearchModes.Parent:
					//return the first component found in parent
					T parentComponent = gameObject.GetComponentInParent<T>();
					if(parentComponent != null)
					{
						components = new T[1];
						components[0] = parentComponent;
					}
					else
					{
						components = new T[0];
					}
					break;

				case SearchModes.Parents:
					//return all components found in parent
					components = gameObject.GetComponentsInParent<T>();
					break;

				default:
				case SearchModes.Self:
					//just return the components on this object
					components = gameObject.GetComponents<T>();
					break;

				case SearchModes.Child:
					//return the first component found in children
					T childComponent = gameObject.GetComponentInChildren<T>();
					if(childComponent != null)
					{
						components = new T[1];
						components[0] = childComponent;
					}
					else
					{
						components = new T[0];
					}
					break;

				case SearchModes.Children:
					//return all components found in children
					components = gameObject.GetComponentsInChildren<T>();
					break;

				case SearchModes.Custom:
					//return all components attached to objects in the custom list
					if(customObjects == null)
					{
						components = new T[0];
					}
					else
					{
						List<T> customTs = new List<T>(customObjects.Count);
						for(int i = 0; i < customObjects.Count; i++)
						{
							if(customObjects[i] == null) continue;
							T[] coTs = customObjects[i].GetComponents<T>();
							for(int j = 0; j < coTs.Length; j++)
							{
								customTs.Add(coTs[j]);
							}
						}
						components = customTs.ToArray();
					}
					break;
			}

			return components;
		}
	}
}