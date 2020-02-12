using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using WIDVE.Patterns.Commands;

namespace WIDVE.Utilities
{
	/// <summary>
	/// MonoBehaviour that automatically loads scenes from a SceneObjectList.
	/// </summary>
	public class SceneLoader : MonoBehaviour
	{
		/// <summary>
		/// Times in the MonoBehaviour lifecycle when scenes can be loaded.
		/// </summary>
		public enum LoadTimes { Manually, OnAwake, OnStart, OnFirstUpdate }

		[SerializeField]
		LoadTimes _scenesWillLoad = LoadTimes.Manually;
		LoadTimes ScenesWillLoad => _scenesWillLoad;

		/// <summary>
		/// Returns true if scenes are loaded.
		/// </summary>
		bool Loaded = false;

		[SerializeField]
		LoadSceneMode _mode = LoadSceneMode.Additive;
		LoadSceneMode Mode => _mode;

		[SerializeField]
		[Tooltip("Scenes will load during play mode in builds. Check this to also load scenes during play mode in the editor.")]
		bool _loadInEditor = false;
		bool LoadInEditor => _loadInEditor;

		[SerializeField]
		[Tooltip("List of scenes to load.")]
		SceneObjectList _scenes;
		SceneObjectList Scenes => _scenes;

		[SerializeField]
		[Tooltip("[Optional] CommandHistory to store scene load actions.")]
		CommandHistory _sceneLoadHistory;
		CommandHistory SceneLoadHistory => _sceneLoadHistory;

		/// <summary>
		/// Returns true if scenes should be loaded, false otherwise.
		/// </summary>
		bool ShouldLoad
		{
			get
			{
				if (!enabled) return false;
				if (!LoadInEditor && !Application.IsPlaying(this)) return false;
				if (Application.isEditor && !LoadInEditor) return false;
				return true;
			}
		}

		/// <summary>
		/// Load all scenes referenced by this SceneLoader.
		/// </summary>
		public void Load()
		{
			if(!Scenes)
			{
				Debug.LogError($"Error! {name} can't load scenes; no SceneObjectList.");
				return;
			}

			SceneObject.SceneObjectLoaded += CheckActiveSceneOnLoad;
			Scenes.LoadAll(Mode, SceneLoadHistory);
			Loaded = true;
		}

		/// <summary>
		/// Unload all scenes referenced by this SceneLoader.
		/// </summary>
		public void Unload()
		{
			if(!Scenes)
			{
				Debug.LogError($"Error! {name} can't unload scenes; no SceneObjectList.");
				return;
			}

			Scenes.UnloadAll(Mode, SceneLoadHistory);
			Loaded = false;
		}

		void CheckActiveSceneOnLoad(Scene scene, LoadSceneMode loadSceneMode)
		{	//set the active scene if it was loaded
			if(Scenes.ActiveScene == null)
			{   //no active scene specified, so stop checking
				SceneObject.SceneObjectLoaded -= CheckActiveSceneOnLoad;
			}
			else if(scene == Scenes.ActiveScene.GetScene())
			{
				SceneManager.SetActiveScene(scene);
				//active scene has been set, so done checking
				SceneObject.SceneObjectLoaded -= CheckActiveSceneOnLoad;
			}
		}

		//when specified, load scenes during MonoBehaviour lifecycle
		void Awake()
		{
			if (!ShouldLoad) return;

			if (!Loaded && ScenesWillLoad == LoadTimes.OnAwake)
			{
				Load();
			}
		}

		void Start()
		{
			if (!ShouldLoad) return;

			if (!Loaded && ScenesWillLoad == LoadTimes.OnStart)
			{
				Load();
			}
		}

		void Update()
		{
			if (!ShouldLoad) return;

			if (!Loaded && ScenesWillLoad == LoadTimes.OnFirstUpdate)
			{
				Load();
			}

			//can sleep now
			enabled = false;
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(SceneLoader))]
		public class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				if (GUILayout.Button("Load"))
				{
					foreach(Object t in targets)
					{
						SceneLoader sl = t as SceneLoader;
						sl.Load();
					}
				}

				if (GUILayout.Button("Unload"))
				{
					foreach(Object t in targets)
					{
						SceneLoader sl = t as SceneLoader;
						sl.Unload();
					}
				}
			}
		}
#endif
	}
}