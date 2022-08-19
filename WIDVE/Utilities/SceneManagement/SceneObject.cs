//Copyright WID Virtual Environments Group 2018-Present
//Simon Smith
//Ross Tredinnick

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using WIDVE.Patterns;

namespace WIDVE.Utilities
{
	/// <summary>
	/// ScriptableObject class to store Scene data persistently, in the editor and in builds.
	/// </summary>
	[CreateAssetMenu(fileName = nameof(SceneObject), menuName = nameof(Utilities) + "/" + nameof(SceneObject), order = WIDVEEditor.C_ORDER)]
	public class SceneObject : ScriptableObject
#if UNITY_EDITOR
							 , ISerializationCallbackReceiver
#endif
	{
		//partially based on https://github.com/ZeShmoutt/Unity-Scene-ScriptableObject

#if UNITY_EDITOR
		//SceneAssets only exist in the Editor
		[SerializeField]
		SceneAsset _scene;
		SceneAsset Scene => _scene;
#endif

		[SerializeField]
		string _scenePath;
		/// <summary>
		/// Path to Scene asset.
		/// </summary>
		public string ScenePath
		{
			get => _scenePath;
			private set => _scenePath = value;
		}

		[SerializeField]
		string _name;
		/// <summary>
		/// Name of Scene asset.
		/// </summary>
		public string Name
		{
			get => _name;
			private set => _name = value;
		}

		[SerializeField]
		int _index;
		/// <summary>
		/// Scene asset's index in build order.
		/// <para>Try to avoid using this - this is being set incorrectly when building.</para>
		/// </summary>
		/*public*/ int Index //making this private as long as it's broken
		{
			get => _index;
			/*private*/ set => _index = value;
		}

		/// <summary>
		/// Returns true if the Scene is loaded, false otherwise.
		/// </summary>
		public bool IsLoaded
		{
			get
			{
				Scene scene = GetScene();
				if(scene != null) return scene.isLoaded;
				else return false;
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Invoked whenever a SceneObject has been opened in the hierarchy.
		/// </summary>
		public static event UnityAction<Scene, OpenSceneMode> SceneObjectOpened;

		/// <summary>
		/// Invoked whenever a SceneObject has been closed in the hierarchy.
		/// </summary>
		public static event UnityAction<Scene> SceneObjectClosed;
#endif

		/// <summary>
		/// Invoked whenever a SceneObject has finished loading a Scene.
		/// </summary>
		public static event UnityAction<Scene, LoadSceneMode> SceneObjectLoaded;

		/// <summary>
		/// Invoked whenever a SceneObject has finished unloading a Scene.
		/// </summary>
		public static event UnityAction<Scene> SceneObjectUnloaded;

#if UNITY_EDITOR
		/// <summary>
		/// Converts a LoadSceneMode value into an OpenSceneMode value.
		/// </summary>
		public static OpenSceneMode GetOpenSceneMode(LoadSceneMode loadSceneMode)
		{
			switch (loadSceneMode)
			{
				default:
				case LoadSceneMode.Additive:
					return OpenSceneMode.Additive;
				case LoadSceneMode.Single:
					return OpenSceneMode.Single;
			}
		}
#endif

		/// <summary>
		/// Returns the Scene used by this SceneObject.
		/// </summary>
		/// <returns></returns>
		public Scene GetScene()
		{
#if UNITY_EDITOR
			return EditorSceneManager.GetSceneByPath(ScenePath);
#endif
			//return SceneManager.GetSceneByBuildIndex(Index);

			//not sure why, but buildIndex is not being set correctly when building...
			//just use the path instead
			return SceneManager.GetSceneByPath(ScenePath);
		}

#if UNITY_EDITOR
		/// <summary>
		/// Refresh SceneObject with current data from Scene asset.
		/// </summary>
		[ContextMenu("Sync")]
		public void Sync()
		{
			SyncDataToSceneAsset(Scene);
		}

		void SyncDataToSceneAsset(SceneAsset scene)
		{
			if (scene)
			{
				//get current info from scene
				Name = scene.name;
				Index = SceneManager.GetSceneByName(Name).buildIndex;
				ScenePath = AssetDatabase.GetAssetOrScenePath(scene);
			}

			else
			{
				//set default values if no scene is available
				Name = "NULL";
				Index = -2; //set this to -2 because -1 already has a special meaning (see below)
				ScenePath = string.Empty;
			}
		}
#endif

		/// <summary>
		/// Returns true if this scene can be loaded by index.
		/// </summary>
		/// <param name="print">Set this to true to print debug messages when a scene cannot be loaded.</param>
		public bool IsLoadableByIndex(bool print=false)
		{  
			//check if scene can be loaded based on index
			//see https://docs.unity3d.com/ScriptReference/SceneManagement.Scene-buildIndex.html
			if(Index == -2)
			{
				//uninitialized SceneObject
				if (print) Debug.LogError($"Scene '{Name}' cannot be loaded by index; SceneObject is invalid.");
				return false;
			}
			else if (Index == -1)
			{
				//scene is in AssetBundle
				if (print) Debug.LogError($"Scene '{Name}' cannot be loaded by index; scene is part of an AssetBundle.");
				return false;
			}
			else if (Index >= 0 && Index < SceneManager.sceneCountInBuildSettings)
			{
				//scene can be loaded
				return true;
			}
			else if(Index >= SceneManager.sceneCountInBuildSettings)
			{
				//scene not included in build settings
				if (print) Debug.LogError($"Scene '{Name}' cannot be loaded by index; scene is not included in build settings.");
				return false;
			}
			else
			{
				//can't load for an unknown reason
				if (print) Debug.LogError($"Scene '{Name}' cannot be loaded by index; unknown error [index: {Index}].");
				return false;
			}
		}

		/// <summary>
		/// Load the scene asynchronously.
		/// </summary>
		public ICommand Load(LoadSceneMode mode=LoadSceneMode.Additive, CommandHistory ch=null)
		{
			ICommand load = new Commands.Load(this, mode);
			CommandHistory.Execute(load, ch);
			return load;
		}

		/// <summary>
		/// Unload the scene asynchronously.
		/// <para>In the Editor, the scene will not be unloaded if has any unsaved changes.</para>
		/// </summary>
		public ICommand Unload(LoadSceneMode mode=LoadSceneMode.Additive, CommandHistory ch=null)
		{
			ICommand unload = new Commands.Unload(this, mode);
			CommandHistory.Execute(unload, ch);
			return unload;
		}

		/// <summary>
		/// Load the scene asynchronously.
		/// <para>In Edit mode, loads synchronously.</para>
		/// </summary>
		/// <param name="mode">Load scene additively or singly.</param>
		/// <returns>The asynchronous load operation.</returns>
		AsyncOperation LoadAsync(LoadSceneMode mode)
		{
#if UNITY_EDITOR
			LoadSceneParameters lsp = new LoadSceneParameters(mode);

			//in editor, load scene by path, since build index is broken...
			if (Application.isPlaying)
			{
				//load async
				return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, lsp);
			}
			else
			{
				//can't load async in edit mode
				EditorSceneManager.OpenScene(ScenePath, GetOpenSceneMode(mode));
				SceneObjectLoaded?.Invoke(GetScene(), mode);
				return null;
			}
#endif

			//buildIndex is being set incorrectly when making builds, so load by path
			return SceneManager.LoadSceneAsync(ScenePath, mode);

			/*
			//in build, load by index
			if (IsLoadableByIndex(print: true))
			{
				return SceneManager.LoadSceneAsync(Index, mode);
			}
			else return null;
			*/
		}

		/// <summary>
		/// Unload the scene asynchronously.
		/// </summary>
		/// <returns>The asynchronous unload operation.</returns>
		AsyncOperation UnloadAsync()
		{
			try
			{
				Scene scene = GetScene();

#if UNITY_EDITOR
				//in editor, unload by path
				if (scene.isLoaded)
				{
					if (scene.isDirty && !Application.isPlaying)
					{
						//don't unload if it would lose unsaved changes
						Debug.LogError($"Please save scene '{Name}' before unloading!");
						return null;
					}

					else
					{
						//unload!
						return SceneManager.UnloadSceneAsync(scene);
					}
				}

				else
				{
					//scene is not currently loaded...
					if (Application.isPlaying)
					{
						//don't try to unload a scene if it isn't loaded
						return null;
					}

					else
					{
						//remove the scene from the hierarchy
						return SceneManager.UnloadSceneAsync(scene);
					}
				}
#endif

				//buildIndex is being set incorrectly when making builds, so unload by path
				return SceneManager.UnloadSceneAsync(ScenePath);

				/*
				//in build, unload by index
				if (scene.isLoaded)
				{
					if (IsLoadableByIndex())
					{
						return SceneManager.UnloadSceneAsync(Index);
					}
				}
				return null;
				*/
			}

			catch (System.ArgumentException)
			{
				//this happens if the scene is not currently loaded when the unload operation starts
				return null;
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Editor only: add this scene to the hierarchy as an unloaded scene.
		/// </summary>
		/// <returns>The opened Scene.</returns>
		public bool Open()
		{
			bool opened = false;

			Scene scene = GetScene();

			if (!scene.isLoaded)
			{
				Scene openedScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.AdditiveWithoutLoading);
				if(openedScene == scene) opened = true;
			}

			if (opened) SceneObjectOpened?.Invoke(scene, OpenSceneMode.AdditiveWithoutLoading);

			return opened;
		}

		/// <summary>
		/// Editor only: unload this scene without removing its entry from the hierarchy.
		/// <para>The scene will not be unloaded if it has any unsaved changes.</para>
		/// </summary>
		/// <returns>True if scene was closed, false otherwise.</returns>
		public bool Close()
		{
			bool closed = false;

			Scene scene = GetScene();

			if (scene.isLoaded)
			{
				if (scene.isDirty)
				{
					Debug.LogError($"Please save scene '{Name}' before closing!");
				}
				else
				{
					closed = EditorSceneManager.CloseScene(scene, false);
				}
			}

			if (closed) SceneObjectClosed?.Invoke(scene);

			return closed;
		}
#endif

#if UNITY_EDITOR
		//Sync to underlying scene asset when data is serialized
		public void OnBeforeSerialize()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				Sync();
			}
		}

		public void OnAfterDeserialize() { }
#endif

		public class Commands
		{
			/// <summary>
			/// Load scene asynchronously.
			/// </summary>
			public class Load : Command<SceneObject>
			{
				LoadSceneMode Mode;

				/// <summary>
				/// The current asynchronous operation being performed.
				/// </summary>
				public AsyncOperation Operation { get; private set; }

				public Load(SceneObject target, LoadSceneMode mode=LoadSceneMode.Additive) : base(target)
				{
					Mode = mode;
				}

				public override void Execute()
				{
					Operation = Target.LoadAsync(Mode);

					if (Operation != null)
					{
						//scene is being loaded
						Operation.completed += DisplayLoadMessage;
						Operation.completed += InvokeSceneObjectLoaded;
					}
					else
					{
						//this could be null if:
						//	load is called in the editor - this will load without using an AsyncOperation
					}
				}

				public override void Undo()
				{
					Operation = Target.UnloadAsync();

					if (Operation != null)
					{
						//scene is being unloaded
						Operation.completed += DisplayUnloadMessage;
						Operation.completed += InvokeSceneObjectUnloaded;

						//todo: on non-additive Undo, reload the previously active scene
					}
					else
					{
						//this could happen for several reasons:
						//	trying to unload a scene with unsaved changes
						//	trying to unload a scene that has already been unloaded
						//	trying to unload a scene that is currently loading
						//	trying to unload the only active scene
					}
					
				}

				void DisplayLoadMessage(AsyncOperation operation)
				{
					Debug.Log($"'{Target.name}' loaded.");
				}

				void DisplayUnloadMessage(AsyncOperation operation)
				{
					Debug.Log($"'{Target.name}' unloaded.");
				}

				void InvokeSceneObjectLoaded(AsyncOperation operation)
				{
					SceneObjectLoaded?.Invoke(Target.GetScene(), Mode);
				}

				void InvokeSceneObjectUnloaded(AsyncOperation operation)
				{
					SceneObjectUnloaded?.Invoke(Target.GetScene());
				}
			}

			/// <summary>
			/// Unload scene asynchronously.
			/// <para>Just the opposite of the Load command.</para>
			/// </summary>
			public class Unload : Load
			{
				public Unload(SceneObject target, LoadSceneMode mode=LoadSceneMode.Additive) : base(target, mode) { }

				public override void Execute()
				{
					base.Undo();
				}

				public override void Undo()
				{	
					base.Execute();
				}
			}

		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(SceneObject), true)]
		public class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				//the scene field is the only one that can be modified
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_scene)));

				bool changed = EditorGUI.EndChangeCheck();

				//other fields are set during Sync, so don't let the user edit them
				GUI.enabled = false;

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_scenePath)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_name)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_index)));

				GUI.enabled = true;

				serializedObject.ApplyModifiedProperties();

				//sync if the scene changed
				if(changed)
				{
					foreach(SceneObject so in targets)
					{
						so.Sync();
					}
				}

				/*
				if (!Application.isPlaying)
				{
					//press this button to manually sync (for debugging)
					if (GUILayout.Button("Refresh"))
					{
						foreach(SceneObject so in targets)
						{
							so.Sync();
						}
					}
				}
				*/

				//open and close the scene in the hierarchy view
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Open"))
				{
					foreach(SceneObject so in targets)
					{
						so.Open();
					}
				}

				if (GUILayout.Button("Close"))
				{
					foreach(SceneObject so in targets)
					{
						so.Close();
					}
				}

				GUILayout.EndHorizontal();

				//load and unload the scene additively
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Load"))
				{
					foreach(SceneObject so in targets)
					{
						so.Load(LoadSceneMode.Additive);
					}
				}

				if (GUILayout.Button("Unload"))
				{
					foreach(SceneObject so in targets)
					{
						so.Unload(LoadSceneMode.Additive);
					}
				}

				GUILayout.EndHorizontal();

				//set active scene - only on the displayed SceneObject, not all selected SceneObjects
				if(GUILayout.Button("Set Active"))
				{
					SceneObject so = target as SceneObject;
					SceneManager.SetActiveScene(so.GetScene());
				}
			}
		}
#endif
	}
}