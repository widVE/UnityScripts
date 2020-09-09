using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using WIDVE.Utilities;

namespace WIDVE.Graphics
{
	public abstract class RendererController : MonoBehaviour, IInterpolatable //make this inherit from the generic Controller class now...
	{
		[SerializeField]
		[Tooltip("ShaderProperties used by all Renderers controlled by this behaviour.")]
		protected ShaderProperties _shaderProperties;
		protected ShaderProperties ShaderProperties => _shaderProperties;

		[SerializeField]
		[Tooltip("Specifies which Renderers are affected by this object.")]
		protected GameObjectExtensions.SearchModes _mode = GameObjectExtensions.SearchModes.Self;
		protected GameObjectExtensions.SearchModes Mode => _mode;

		[SerializeField]
		[HideInInspector]
		protected List<GameObject> _objects;
		protected List<GameObject> Objects => _objects ?? (_objects = new List<GameObject>());

		[SerializeField]
		[HideInInspector]
		protected float _currentValue;
		/// <summary>
		/// The current value between 0 and 1 that this component is at.
		/// </summary>
		protected float CurrentValue
		{
			get => _currentValue;
			set => _currentValue = value;
		}

		static MaterialPropertyBlock _mpb;
		/// <summary>
		/// Reusable MaterialPropertyBlock.
		/// </summary>
		protected static MaterialPropertyBlock MPB => _mpb ?? (_mpb = new MaterialPropertyBlock());

		Renderer[] _renderers;
		/// <summary>
		/// All Renderers controlled by this component.
		/// </summary>
		protected Renderer[] Renderers
		{
			get
			{
				if(_renderers == null || _renderers.Length == 0) _renderers = GetRenderers();
				return _renderers;
			}
			private set => _renderers = value;
		}

		public bool Enabled => enabled;

		public virtual bool FunctionWhenDisabled => false;

		protected Renderer[] GetRenderers()
		{
			return gameObject.GetComponentsInHierarchy<Renderer>(Mode, Objects);
		}

		protected virtual void SetMaterial(Material m)
		{
			for(int i = 0; i < Renderers.Length; i++)
			{
				Renderers[i].sharedMaterial = m;
				ShaderProperties.SetRenderModes(m, Renderers[i].sharedMaterial);
			}
		}

		protected void RemoveMaterialPropertyBlock()
		{
			for(int i = 0; i < Renderers.Length; i++)
			{
				Renderers[i].SetPropertyBlock(null);
			}
		}

		public abstract void SetValue(float value);

#if UNITY_EDITOR
		[ContextMenu("Refresh Renderers")]
		void RefreshRenderers()
		{
			Renderers = GetRenderers();
		}

		[CanEditMultipleObjects]
		[CustomEditor(typeof(RendererController), true)]
		protected class Editor : UnityEditor.Editor
		{
			ReorderableList Objects;

			protected bool DrawRCInspector()
			{
				EditorGUI.BeginChangeCheck();

				serializedObject.Update();

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_shaderProperties)));

				SerializedProperty mode = serializedObject.FindProperty(nameof(_mode));
				EditorGUILayout.PropertyField(mode);

				if(mode.enumValueIndex == (int)GameObjectExtensions.SearchModes.Custom)
				{
					Objects.DoLayoutList();
				}

				serializedObject.ApplyModifiedProperties();

				bool somethingChanged = EditorGUI.EndChangeCheck();

				if(somethingChanged)
				{
					foreach(RendererController rc in targets)
					{
						rc.Renderers = rc.GetRenderers();
					}
				}

				return somethingChanged;
			}

			protected virtual void OnEnable()
			{
				Objects = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(_objects)),
											true, true, true, true);

				Objects.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Objects");
				};

				Objects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = Objects.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
											property: element,
											label: new GUIContent(index.ToString()));
				};
			}

			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				serializedObject.Update();

				DrawDefaultInspector();

				SerializedProperty mode = serializedObject.FindProperty(nameof(_mode));

				if(mode.enumValueIndex == (int)GameObjectExtensions.SearchModes.Custom)
				{
					Objects.DoLayoutList();
				}

				serializedObject.ApplyModifiedProperties();

				bool somethingChanged = EditorGUI.EndChangeCheck();

				if(somethingChanged)
				{
					foreach(RendererController rc in targets)
					{
						rc.Renderers = rc.GetRenderers();
					}					
				}
			}
		}
#endif
	}
}