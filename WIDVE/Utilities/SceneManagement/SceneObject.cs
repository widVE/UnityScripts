using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using WIDVE.Patterns.Commands;

namespace WIDVE.Utilities
{
	/// <summary>
	/// ScriptableObject class to store Scene data persistently, in the editor and in builds.
	/// </summary>
	[CreateAssetMenu(fileName ="SceneObject", menuName = nameof(Utilities) + "/SceneObject", order = WIDVEEditor.C_ORDER)]
	public class SceneObject : ScriptableObject
#if UNITY_EDITOR
							 , ISerializationCallbackReceiver
#endif
	{   //partially based on https://github.com/ZeShmoutt/Unity-Scene-ScriptableObject
#if UNITY_EDITOR
		//SceneAssets only exist in the Editor
		[SerializeField]
		SceneAsset Scene; //set in Inspector
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
		/// <para>Try to avoid using this - this is being set incorrectly in builds.</para>
		/// </summary>
		public int Index
		{
			get => _index;
			private set => _index = value;
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
			//not sure why, but buildIndex is not being set correctly when building...
			//return SceneManager.GetSceneByBuildIndex(Index);
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
			{	//get current info from scene
				Name = scene.name;
				Index = SceneManager.GetSceneByName(Name).buildIndex;
				ScenePath = AssetDatabase.GetAssetOrScenePath(scene);
			}
			else
			{	//set default values if no scene is available
				Name = "NULL";
				Index = -2; //set this to -2 because -1 already has a special meaning (see below)
				ScenePath = null;
			}
		}
#endif

		/// <summary>
		/// Returns true if this scene can be loaded by index.
		/// </summary>
		/// <param name="print">Set this to true to print debug messages when a scene cannot be loaded.</param>
		public bool IsLoadableByIndex(bool print=false)
		{   //check if scene can be loaded based on index
			//see https://docs.unity3d.com/ScriptReference/SceneManagement.Scene-buildIndex.html
			if(Index == -2)
			{   //uninitialized SceneObject
				if (print) Debug.LogError($"Scene '{Name}' cannot be loaded by index; SceneObject is invalid.");
				return false;
			}
			else if (Index == -1)
			{   //scene is in AssetBundle
				if (print) Debug.LogError($"Scene '{Name}' cannot be loaded by index; scene is part of asset bundle.");
				return false;
			}
			else if (Index >= 0 && Index < SceneManager.sceneCountInBuildSettings)
			{   //scene can be loaded
				return true;
			}
			else if(Index >= SceneManager.sceneCountInBuildSettings)
			{   //scene not included in build settings
				if (print) Debug.LogError($"Scene '{Name}' cannot be loaded by index; scene is not included in build settings.");
				return false;
			}
			else
			{   //can't load for an unknown reason
				if (print) Debug.LogError($"Scene '{Name}' cannot be loaded by index; unknown error.");
				return false;
			}
		}

		/// <summary>
		/// Load the scene asynchronously.
		/// </summary>
		public Commands.Load Load(LoadSceneMode mode=LoadSceneMode.Additive, CommandHistory ch=null)
		{
			Commands.Load load = new Commands.Load(this, mode);
			CommandHistory.Execute(load, ch);
			return load;
		}

		/// <summary>
		/// Unload the scene asynchronously.
		/// <para>Editor only: The scene will not be unloaded if has any unsaved changes.</para>
		/// </summary>
		public Commands.Unload Unload(LoadSceneMode mode=LoadSceneMode.Additive, CommandHistory ch=null)
		{
			Commands.Unload unload = new Commands.Unload(this, mode);
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
			//in editor, load by path
			LoadSceneParameters lsp = new LoadSceneParameters(mode);
			if (Application.isPlaying)
			{	//load async
				return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, lsp);
			}
			else
			{	//can't load async in the editor...
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
					{	//don't unload if it would lose unsaved changes
						Debug.LogError($"Please save scene '{Name}' before unloading!");
						return null;
					}
					else
					{
						return SceneManager.UnloadSceneAsync(scene);
					}
				}
				else
				{	//scene is not currently loaded...
					if (Application.isPlaying)
					{	//don't try to unload a scene if it isn't already loaded
						return null;
					}
					else
					{   //remove the scene from the hierarchy
						return SceneManager.UnloadSceneAsync(scene);
					}
				}
#endif
				//buildIndex is being set incorrectly when making builds, so load by path
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
			catch (System.ArgumentException ae)
			{   //this happens if the scene is not currently loaded when the unload operation starts
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
				EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.AdditiveWithoutLoading);
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
		//Sync to underlying scene asset when these Unity events happen
		public void OnBeforeSerialize()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				Sync();
			}
		}

		public void OnAfterDeserialize()
		{	//syncing here causes a serialization error...
			//Sync();
		}

		void OnValidate()
		{   //undocumented, but OnValidate -is- called on ScriptableObjects
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				Sync();
			}
		}
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
					Debug.Log($"Loading scene '{Target.Name}'...");
					Operation = Target.LoadAsync(Mode);
					if (Operation != null)
					{	//scene is being loaded
						Operation.completed += InvokeSceneObjectLoaded;
					}
					else
					{   //this could be null if:
						//	scene is loaded in the editor - this will load without using an AsyncOperation
						//	something went wrong with loading
					}
				}

				public override void Undo()
				{
					Debug.Log($"Unloading scene '{Target.Name}'...");
					Operation = Target.UnloadAsync();
					if (Operation != null)
					{	//scene is being unloaded
						Operation.completed += InvokeSceneObjectUnloaded;
						//todo: on non-additive Undo, reload the previously active scene
					}
					else
					{   //this could happen for several reasons:
						//	trying to unload a scene with unsaved changes
						//	trying to unload a scene that has already been unloaded
						//	trying to unload a scene that is currently loading
						//	trying to unload the only active scene
					}
					
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
		[CustomEditor(typeof(SceneObject))]
		public class Editor : UnityEditor.Editor
		{
			SceneObject Target;
			SerializedProperty Scene;
			SerializedProperty ScenePath;
			SerializedProperty Name;
			SerializedProperty Index;

			void OnEnable()
			{
				Target = target as SceneObject;

				Scene = serializedObject.FindProperty(nameof(SceneObject.Scene));
				ScenePath = serializedObject.FindProperty(nameof(SceneObject._scenePath));
				Name = serializedObject.FindProperty(nameof(SceneObject._name));
				Index = serializedObject.FindProperty(nameof(SceneObject._index));
			}

			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				EditorGUILayout.PropertyField(Scene);

				//these are set during Sync, so don't let the user edit them
				GUI.enabled = false;
				EditorGUILayout.PropertyField(ScenePath);
				EditorGUILayout.PropertyField(Name);
				EditorGUILayout.PropertyField(Index);
				GUI.enabled = true;

				//show buttons
				if (!Application.isPlaying)
				{   //press this button to manually sync (for debugging)
					if (GUILayout.Button("Refresh"))
					{
						Target.Sync();
					}
				}
				if (GUILayout.Button("Open"))
				{
					Target.Open();
				}
				if (GUILayout.Button("Close"))
				{
					Target.Close();
				}
				if (GUILayout.Button("Load"))
				{
					Target.Load(LoadSceneMode.Additive);
				}
				if (GUILayout.Button("Unload"))
				{
					Target.Unload(LoadSceneMode.Additive);
				}

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}