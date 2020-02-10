using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics
{
	[CreateAssetMenu(fileName = nameof(ShaderProperties), menuName = nameof(WIDVE.Graphics) + "/" + nameof(ShaderProperties), order = 1500)]
	public class ShaderProperties : ScriptableObject
	{
		[System.Serializable]
		public abstract class ShaderProperty
		{
			[SerializeField]
			Shader _shader;
			/// <summary>
			/// Shader this property belongs to.
			/// </summary>
			public Shader Shader
			{
				get => _shader;
				private set => _shader = value;
			}

			[SerializeField]
			string _name;
			/// <summary>
			/// Name of this property in the shader.
			/// </summary>
			public string Name
			{
				get => _name;
				private set => _name = value;
			}

			public ShaderProperty(string name, Shader shader)
			{
				Name = name;
				Shader = shader;
			}
		}

		[System.Serializable]
		public abstract class ShaderProperty<T> : ShaderProperty
		{
			public ShaderProperty(string name, Shader shader) : base(name, shader) { }

			/// <summary>
			/// Returns the value of this property on the given Material.
			/// </summary>
			public abstract T Get(Material m);

			/// <summary>
			/// Sets the property on the Material to the given value.
			/// </summary>
			public abstract void Set(Material m, T t);

			/// <summary>
			/// Returns the lerp of this property between the given Materials at time t.
			/// </summary>
			public abstract T Lerp(Material a, Material b, float t);
		}

		[System.Serializable]
		public class ColorProperty : ShaderProperty<Color>
		{
			public ColorProperty(string name, Shader shader) : base(name, shader) { }

			public override Color Get(Material m)
			{
				return m.GetColor(Name);
			}

			public override void Set(Material m, Color c)
			{
				m.SetColor(Name, c);
			}

			public override Color Lerp(Material m1, Material m2, float t)
			{
				return Color.Lerp(Get(m1), Get(m2), t);
			}
		}

		[System.Serializable]
		public class FloatProperty : ShaderProperty<float>
		{
			[SerializeField]
			bool _isEnum;
			public bool IsEnum
			{
				get => _isEnum;
				private set => _isEnum = value;
			}

			public FloatProperty(string name, Shader shader, bool isEnum) : base(name, shader)
			{
				IsEnum = isEnum;
			}

			public override float Get(Material m)
			{
				return m.GetFloat(Name);
			}

			public override void Set(Material m, float f)
			{
				m.SetFloat(Name, f);
			}

			public override float Lerp(Material m1, Material m2, float t)
			{   //don't try to lerp enums - just return the value from material a
				if(IsEnum) return Get(m1);
				else return Mathf.Lerp(Get(m1), Get(m2), t);
			}
		}

		[System.Serializable]
		public class VectorProperty : ShaderProperty<Vector4>
		{
			public VectorProperty(string name, Shader shader) : base(name, shader) { }

			public override Vector4 Get(Material m)
			{
				return m.GetVector(Name);
			}

			public override void Set(Material m, Vector4 v)
			{
				m.SetVector(Name, v);
			}

			public override Vector4 Lerp(Material m1, Material m2, float t)
			{
				return Vector4.Lerp(Get(m1), Get(m2), t);
			}
		}

		[System.Serializable] //texture property can't lerp yet...
		public class TextureProperty : ShaderProperty<Texture>
		{
			public TextureProperty(string name, Shader shader) : base(name, shader) { }

			public override Texture Get(Material m)
			{
				return m.GetTexture(Name);
			}

			public override void Set(Material m, Texture t)
			{
				m.SetTexture(Name, t);
			}

			public override Texture Lerp(Material a, Material b, float t)
			{   //texture lerping not yet implemented
				throw new System.NotImplementedException();
			}
		}
		const bool USE_TEXTURES = false;

		[SerializeField]
		Shader _sourceShader;
		public Shader SourceShader => _sourceShader;

		[SerializeField]
		List<ColorProperty> _colorProperties;
		public List<ColorProperty> ColorProperties => _colorProperties ?? (_colorProperties = new List<ColorProperty>());

		[SerializeField]
		List<FloatProperty> _floatProperties;
		public List<FloatProperty> FloatProperties => _floatProperties ?? (_floatProperties = new List<FloatProperty>());

		[SerializeField]
		List<VectorProperty> _vectorProperties;
		public List<VectorProperty> VectorProperties => _vectorProperties ?? (_vectorProperties = new List<VectorProperty>());

		[SerializeField]
		List<TextureProperty> _textureProperties;
		public List<TextureProperty> TextureProperties => _textureProperties ?? (_textureProperties = new List<TextureProperty>());

		void SetPropertiesFromShader()
		{
			if(SourceShader) SetPropertiesFromShader(SourceShader);
			else Debug.Log("Need to set a source shader!");
		}

		/// <summary>
		/// Stores all properties found in the shader.
		/// <para>Only functions in the Editor (ShaderUtil is unavailable outside Editor).</para>
		/// </summary>
		void SetPropertiesFromShader(Shader shader)
		{
#if UNITY_EDITOR
			//open shader file to read attributes (necessary for enum checking)
			string shaderPath = AssetDatabase.GetAssetPath(shader);
			using(StreamReader sr = new StreamReader(shaderPath))
			{   //reset property lists
				ColorProperties.Clear();
				FloatProperties.Clear();
				VectorProperties.Clear();
				TextureProperties.Clear();

				//read all properties from shader and add them to the right list
				for(int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
				{
					ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
					string name = ShaderUtil.GetPropertyName(shader, i);
					switch(type)
					{
						case ShaderUtil.ShaderPropertyType.Color:
							ColorProperties.Add(new ColorProperty(name, shader));
							break;
						case ShaderUtil.ShaderPropertyType.Range:
						case ShaderUtil.ShaderPropertyType.Float:
							//check if float is an enum:
							//this doesn't cover all cases! just a quick check
							bool isEnum = false;
							string line;
							while((line = sr.ReadLine()) != null)
							{	//check all lines with attributes
								line = line.Trim(); //remove whitespace from start and end
								if(line.StartsWith("//")) continue; //skip comments
								if(line.StartsWith("["))
								{   //does this line declare the shader variable?
									if(line.Contains(name))
									{   //check if it has an enum attribute
										if(line.StartsWith("[Enum") || line.StartsWith("[Toggle"))
										{
											isEnum = true;
										}
										//don't check any other lines
										break;
									}
									//could add an additional check for an attribute by itself, on the line above the property...
								}
							}
							//return to start of file
							sr.DiscardBufferedData();
							sr.BaseStream.Seek(0, SeekOrigin.Begin);
							//now can add the property
							FloatProperties.Add(new FloatProperty(name, shader, isEnum));
							break;
						case ShaderUtil.ShaderPropertyType.Vector:
							VectorProperties.Add(new VectorProperty(name, shader));
							break;
						case ShaderUtil.ShaderPropertyType.TexEnv:
							TextureProperties.Add(new TextureProperty(name, shader));
							break;
						default:
							Debug.Log($"Unsupported shader property '{name}' (type: {type})");
							break;
					}
				}
			}
#else
			Debug.Log("Can't retrieve shader properties outside of edit mode!");
#endif
		}

		/// <summary>
		/// Sets all enum properties on destination Material to match source Material.
		/// <para>Also sets the render queue.</para>
		/// </summary>
		/// <param name="src">Material to get enum values from.</param>
		/// <param name="dst">Material that will have its enum values changed.</param>
		public void SetRenderModes(Material src, Material dst)
		{   //need this because MaterialPropertyBlocks don't affect rendering settings (Cull, Zwrite, etc)
			for(int i = 0; i < FloatProperties.Count; i++)
			{	//set all float properties that represent enums
				FloatProperty fp = FloatProperties[i];
				if(fp.IsEnum) fp.Set(dst, fp.Get(src));
			}
			//finally, set the render queue
			dst.renderQueue = Mathf.Clamp(src.renderQueue, 0, 5000);
		}

		/// <summary>
		/// Sets the given MaterialPropertyBlock to have all properties from the given Material.
		/// </summary>
		public void SetPropertyBlock(MaterialPropertyBlock mpb, Material m)
		{
			for(int i = 0; i < ColorProperties.Count; i++)
			{
				ColorProperty cp = ColorProperties[i];
				mpb.SetColor(cp.Name, cp.Get(m));
			}

			for(int i = 0; i < FloatProperties.Count; i++)
			{
				FloatProperty fp = FloatProperties[i];
				mpb.SetFloat(fp.Name, fp.Get(m));
			}

			for(int i = 0; i < VectorProperties.Count; i++)
			{
				VectorProperty vp = VectorProperties[i];
				mpb.SetVector(vp.Name, vp.Get(m));
			}

			if(USE_TEXTURES)
			{
				for(int i = 0; i < TextureProperties.Count; i++)
				{
					TextureProperty tp = TextureProperties[i];
					mpb.SetTexture(tp.Name, tp.Get(m));
				}
			}
		}

		/// <summary>
		/// Sets the MaterialPropertyBlock to have the lerp of all properties between Materials a and b, at time t.
		/// </summary>
		public void LerpPropertyBlock(MaterialPropertyBlock mpb, Material a, Material b, float t)
		{	//lerp all properties of each type
			for(int i = 0; i < ColorProperties.Count; i++)
			{
				ColorProperty cp = ColorProperties[i];
				mpb.SetColor(cp.Name, cp.Lerp(a, b, t));
			}

			for(int i = 0; i < FloatProperties.Count; i++)
			{
				FloatProperty fp = FloatProperties[i];
				mpb.SetFloat(fp.Name, fp.Lerp(a, b, t));
			}

			for(int i = 0; i < VectorProperties.Count; i++)
			{
				VectorProperty vp = VectorProperties[i];
				mpb.SetVector(vp.Name, vp.Lerp(a, b, t));
			}

			if(USE_TEXTURES)
			{	//currently unsupported...
				for(int i = 0; i < TextureProperties.Count; i++)
				{
					TextureProperty tp = TextureProperties[i];
					mpb.SetTexture(tp.Name, tp.Lerp(a, b, t));
				}
			}
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(ShaderProperties))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				if(GUILayout.Button("Set Properties"))
				{
					foreach(Object t in targets)
					{
						(t as ShaderProperties).SetPropertiesFromShader();
					}
				}

				base.OnInspectorGUI();
			}
		}
#endif
	}
}