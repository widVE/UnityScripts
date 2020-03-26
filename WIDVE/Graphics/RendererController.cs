using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics
{
	public abstract class RendererController : MonoBehaviour, IInterpolatable
	{
		[SerializeField]
		[Tooltip("ShaderProperties used by all Renderers controlled by this behaviour.")]
		ShaderProperties _shaderProperties;
		protected ShaderProperties ShaderProperties => _shaderProperties;

		protected enum RendererModes { Parent, Self, Children }
		[SerializeField]
		[Tooltip("Specifies which Renderers are affected by this object.")]
		RendererModes _rendererMode = RendererModes.Parent;
		protected RendererModes RendererMode => _rendererMode;

		[SerializeField]
		[HideInInspector]
		float _currentValue;
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
			get => _renderers ?? (_renderers = GetRenderers());
			private set => _renderers = value;
		}

		public bool Enabled => enabled;

		public virtual bool FunctionWhenDisabled => false;

		Renderer[] GetRenderers()
		{
			Renderer[] renderers = new Renderer[0];
			Renderer r;
			switch(RendererMode)
			{
				case RendererModes.Parent:
					r = GetComponentInParent<Renderer>();
					if(r != null)
					{
						renderers = new Renderer[1];
						renderers[0] = r;
					}
					break;

				default:
				case RendererModes.Self:
					r = GetComponent<Renderer>();
					if(r != null)
					{
						renderers = new Renderer[1];
						renderers[0] = r;
					}
					break;

				case RendererModes.Children:
					renderers = GetComponentsInChildren<Renderer>(true);
					break;
			}
			return renderers;
		}

		public abstract void SetValue(float value);

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(RendererController), true)]
		protected class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

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