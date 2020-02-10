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
		LoadTimes ScenesWillLoad = LoadTimes.Manually;

		/// <summary>
		/// Returns true if scenes are loaded.
		/// </summary>
		bool Loaded = false;
		[SerializeField]
		LoadSceneMode Mode = LoadSceneMode.Additive;
		[SerializeField][Tooltip("Scenes will load during play mode in builds. Set this to true to also load scenes during play mode in the editor.")]
		bool LoadInEditor = false;
		bool ShouldLoad
		{
			get
			{
				if (!enabled) return false;
				if (!Application.IsPlaying(this)) return false;
				if (Application.isEditor && !LoadInEditor) return false;
				return true;
			}
		}
		[SerializeField][Tooltip("List of scenes to load.")]
		SceneObjectList _scenes;
		SceneObjectList Scenes => _scenes;
		[SerializeField]
		[Tooltip("Optional: CommandHistory to store scene load actions.")]
		CommandHistory SceneLoadHistory;

		/// <summary>
		/// Load all scenes referenced by this SceneLoader.
		/// </summary>
		public void Load()
		{
			if (Scenes)
			{
				SceneObject.SceneObjectLoaded += CheckActiveSceneOnLoad;
				Scenes.LoadAll(Mode, SceneLoadHistory);
				Loaded = true;
			}
			else
			{
				Debug.LogError($"Error! {name} can't load scenes; no SceneObjectList.");
			}
		}

		/// <summary>
		/// Unload all scenes referenced by this SceneLoader.
		/// </summary>
		public void Unload()
		{
			if (Scenes)
			{
				Scenes.UnloadAll(Mode, SceneLoadHistory);
				Loaded = false;
			}
			else
			{
				Debug.LogError($"Error! {name} can't unload scenes; no SceneObjectList.");
			}
		}

		void CheckActiveSceneOnLoad(Scene scene, LoadSceneMode loadSceneMode)
		{	//set the active scene if it was loaded
			if(scene == Scenes[Scenes.ActiveScene].GetScene())
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
		[CustomEditor(typeof(SceneLoader))]
		public class Editor : UnityEditor.Editor
		{
			SceneLoader Target;
			SerializedProperty ScenesWillLoad;
			SerializedProperty Mode;
			SerializedProperty LoadInEditor;
			SerializedProperty SceneLoadHistory;
			SerializedProperty Scenes;

			protected virtual void OnEnable()
			{
				Target = target as SceneLoader;

				ScenesWillLoad = serializedObject.FindProperty(nameof(SceneLoader.ScenesWillLoad));
				Mode = serializedObject.FindProperty(nameof(SceneLoader.Mode));
				LoadInEditor = serializedObject.FindProperty(nameof(SceneLoader.LoadInEditor));
				SceneLoadHistory = serializedObject.FindProperty(nameof(SceneLoader.SceneLoadHistory));
				Scenes = serializedObject.FindProperty(nameof(SceneLoader._scenes));
			}

			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				EditorGUILayout.PropertyField(ScenesWillLoad);
				EditorGUILayout.PropertyField(Mode);
				EditorGUILayout.PropertyField(LoadInEditor);
				EditorGUILayout.PropertyField(Scenes);
				EditorGUILayout.PropertyField(SceneLoadHistory);

				//show load, unload buttons in Play mode
				if (GUILayout.Button("Load"))
				{
					Target.Load();
				}
				if (GUILayout.Button("Unload"))
				{
					Target.Unload();
				}

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}