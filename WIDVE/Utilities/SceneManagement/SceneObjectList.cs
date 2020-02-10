using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using WIDVE.Patterns.Commands;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(SceneObjectList), menuName = nameof(Utilities) + "/" + nameof(SceneObjectList), order = UtilitiesEditor.ORDER)]
	public class SceneObjectList : ScriptableObjectList<SceneObject>
	{
		[SerializeField]
		int _activeScene = 0;
		public int ActiveScene => _activeScene;

		[SerializeField]
		List<SceneObject> _objects = new List<SceneObject>();
		protected override List<SceneObject> Objects => _objects ?? (_objects = new List<SceneObject>());
		protected override string SerializedListName => nameof(_objects);

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
				SceneObject scene = Load(i, mode, ch);
			}
		}

		public void UnloadAll(LoadSceneMode mode = LoadSceneMode.Additive, CommandHistory ch = null)
		{
			for (int i = 0; i < Count; i++)
			{
				SceneObject scene = Unload(i, mode, ch);
			}
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(SceneObjectList))]
		new public class Editor : ScriptableObjectList<SceneObject>.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_activeScene)));
				serializedObject.ApplyModifiedProperties();

				base.OnInspectorGUI();
			}
		}
#endif
	}
}