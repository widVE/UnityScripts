﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using WIDVE.Patterns;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(SceneObjectList), menuName = nameof(Utilities) + "/" + nameof(SceneObjectList), order = WIDVEEditor.C_ORDER)]
	public class SceneObjectList : ScriptableObjectList<SceneObject>
	{
		[SerializeField]
		int _activeSceneIndex = 0;
		int ActiveSceneIndex => _activeSceneIndex;

		[SerializeField]
		List<SceneObject> _objects = new List<SceneObject>();
		protected override List<SceneObject> Objects => _objects ?? (_objects = new List<SceneObject>());
		protected override string SerializedListName => nameof(_objects);

		/// <summary>
		/// Returns the SceneObject in this list marked as the active scene.
		/// <para>If there is no active scene specified, returns null.</para>
		/// </summary>
		public SceneObject ActiveScene
		{
			get
			{
				if(ActiveSceneIndex >= 0 && ActiveSceneIndex < Objects.Count) return this[ActiveSceneIndex];
				else return null;
			}
		}

		public SceneObject Load(int index, LoadSceneMode mode = LoadSceneMode.Additive, CommandHistory ch = null)
		{
			SceneObject loadMe = this[index];
			loadMe.Load(mode, ch);
			return loadMe;
		}

		public SceneObject Unload(int index, LoadSceneMode mode = LoadSceneMode.Additive, CommandHistory ch = null)
		{
			SceneObject unloadMe = this[index];
			unloadMe.Unload(mode, ch);
			return unloadMe;
		}

		public void LoadAll(LoadSceneMode mode = LoadSceneMode.Additive, CommandHistory ch = null)
		{
			for (int i = 0; i < Count; i++)
			{
				Load(i, mode, ch);
			}
		}

		public void UnloadAll(LoadSceneMode mode = LoadSceneMode.Additive, CommandHistory ch = null)
		{
			for (int i = 0; i < Count; i++)
			{
				Unload(i, mode, ch);
			}
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(SceneObjectList))]
		new public class Editor : ScriptableObjectList<SceneObject>.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_activeSceneIndex)));
				serializedObject.ApplyModifiedProperties();

				base.OnInspectorGUI();
			}

			protected override string GetElementName(SerializedProperty element, int index)
			{
				return $"{typeof(Scene).Name} {index}{(index == (target as SceneObjectList).ActiveSceneIndex ? " [Active]" : string.Empty)}";
			}
		}
#endif
	}
}