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
		#region ShaderProperty classes
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
			/// <summary>
			/// True if this float represents an enum value.
			/// </summary>
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
			{
				//don't try to lerp enums - just return the value from material 1
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

		[System.Serializable]
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
			{
				//texture lerping not yet implemented
				throw new System.NotImplementedException();
			}
		}
		#endregion

		const bool USE_TEXTURES = false;

		//shader to use
		[SerializeField]
		Shader _sourceShader;
		public Shader SourceShader => _sourceShader;

		//all the properties...
		[SerializeField]
		ColorProperty[] _colorProperties;
		public ColorProperty[] ColorProperties
		{
			get => _colorProperties ?? (_colorProperties = new ColorProperty[0]);
			private set => _colorProperties = value;
		}

		[SerializeField]
		FloatProperty[] _floatProperties;
		public FloatProperty[] FloatProperties
		{
			get => _floatProperties ?? (_floatProperties = new FloatProperty[0]);
			private set => _floatProperties = value;
		}

		[SerializeField]
		VectorProperty[] _vectorProperties;
		public VectorProperty[] VectorProperties
		{
			get => _vectorProperties ?? (_vectorProperties = new VectorProperty[0]);
			private set => _vectorProperties = value;
		}

		[SerializeField]
		TextureProperty[] _textureProperties;
		public TextureProperty[] TextureProperties
		{
			get => _textureProperties ?? (_textureProperties = new TextureProperty[0]);
			private set => _textureProperties = value;
		}

		void SetPropertiesFromShader()
		{
			if(SourceShader)
			{
				int setProperties = SetPropertiesFromShader(SourceShader);
				Debug.Log($"Found {setProperties} properties in '{SourceShader.name}.'");
			}
			else Debug.Log("Need to set a source shader!");
		}

		/// <summary>
		/// Stores all properties found in the shader.
		/// <para>Only functions in the Editor (ShaderUtil is unavailable outside Editor).</para>
		/// </summary>
		int SetPropertiesFromShader(Shader shader)
		{
#if UNITY_EDITOR
			//store properties in lists to start
			List<ColorProperty> colorProperties = new List<ColorProperty>();
			List<FloatProperty> floatProperties = new List<FloatProperty>();
			List<VectorProperty> vectorProperties = new List<VectorProperty>();
			List<TextureProperty> textureProperties = new List<TextureProperty>();

			//open shader file to read attributes (necessary for enum checking)
			string shaderPath = AssetDatabase.GetAssetPath(shader);

			using(StreamReader sr = new StreamReader(shaderPath))
			{
				//read all properties from shader and add them to the right list
				for(int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
				{
					ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
					string name = ShaderUtil.GetPropertyName(shader, i);

					switch(type)
					{
						//Color property:
						case ShaderUtil.ShaderPropertyType.Color:
							colorProperties.Add(new ColorProperty(name, shader));
							break;

						//Float property:
						case ShaderUtil.ShaderPropertyType.Range:
						case ShaderUtil.ShaderPropertyType.Float:
							//check if float is an enum:
							//this doesn't cover all cases! just a quick check
							bool isEnum = false;
							string line;
							while((line = sr.ReadLine()) != null)
							{
								//check all lines with attributes
								line = line.Trim(); //remove whitespace from start and end
								if(line.StartsWith("//")) continue; //skip comments
								if(line.StartsWith("["))
								{  
									//does this line declare the shader variable?
									if(line.Contains(name))
									{
										//check if it has an enum attribute
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
							floatProperties.Add(new FloatProperty(name, shader, isEnum));
							break;

						//Vector property:
						case ShaderUtil.ShaderPropertyType.Vector:
							vectorProperties.Add(new VectorProperty(name, shader));
							break;

						//Texture property:
						case ShaderUtil.ShaderPropertyType.TexEnv:
							textureProperties.Add(new TextureProperty(name, shader));
							break;

						//Unknown property:
						default:
							Debug.Log($"Unsupported shader property '{name}' (type: {type})");
							break;
					}
				}

				//store properties in arrays
				ColorProperties = colorProperties.ToArray();
				FloatProperties = floatProperties.ToArray();
				VectorProperties = vectorProperties.ToArray();
				TextureProperties = textureProperties.ToArray();

				//return the total number of properties
				return ColorProperties.Length + FloatProperties.Length + VectorProperties.Length + TextureProperties.Length;
			}
#else
			Debug.LogError("Error retrieving shader properties; Can't access ShaderUtil outside of edit mode.");
			return 0;
#endif
		}

		/// <summary>
		/// Sets all enum properties on destination Material to match source Material.
		/// <para>Also sets the render queue.</para>
		/// </summary>
		/// <param name="src">Material to get enum values from.</param>
		/// <param name="dst">Material that will have its enum values changed.</param>
		public void SetRenderModes(Material src, Material dst)
		{
			//need to do this because MaterialPropertyBlocks don't affect rendering settings (Cull, Zwrite, etc)
			for(int i = 0; i < FloatProperties.Length; i++)
			{
				//set all float properties that represent enums
				FloatProperty fp = FloatProperties[i];
				if(fp.IsEnum) fp.Set(dst, fp.Get(src));
			}

			//finally, set the render queue
			dst.renderQueue = Mathf.Clamp(src.renderQueue, 0, 5000);
		}

		/// <summary>
		/// Sets the MaterialPropertyBlock to have all properties from the given Material.
		/// </summary>
		public void SetProperties(MaterialPropertyBlock mpb, Material m)
		{
			for(int i = 0; i < ColorProperties.Length; i++)
			{
				ColorProperty cp = ColorProperties[i];
				mpb.SetColor(cp.Name, cp.Get(m));
			}

			for(int i = 0; i < FloatProperties.Length; i++)
			{
				FloatProperty fp = FloatProperties[i];
				mpb.SetFloat(fp.Name, fp.Get(m));
			}

			for(int i = 0; i < VectorProperties.Length; i++)
			{
				VectorProperty vp = VectorProperties[i];
				mpb.SetVector(vp.Name, vp.Get(m));
			}

			if(USE_TEXTURES)
			{
				//this should work, but doesn't... haven't debugged it yet
				for(int i = 0; i < TextureProperties.Length; i++)
				{
					TextureProperty tp = TextureProperties[i];
					mpb.SetTexture(tp.Name, tp.Get(m));
				}
			}
		}

		void SetColorValue(MaterialPropertyBlock mpb, int colorIndex, float value)
		{
			for(int i = 0; i < ColorProperties.Length; i++)
			{
				ColorProperty cp = ColorProperties[i];
				Color c = mpb.GetColor(cp.Name);
				c[colorIndex] = value;
				mpb.SetColor(cp.Name, c);
			}
		}

		/// <summary>
		/// Sets the red value of all colors in the MaterialPropertyBlock.
		/// </summary>
		public void SetRed(MaterialPropertyBlock mpb, float value)		{ SetColorValue(mpb, 0, value); }

		/// <summary>
		/// Sets the green value of all colors in the MaterialPropertyBlock.
		/// </summary>
		public void SetGreen(MaterialPropertyBlock mpb, float value)	{ SetColorValue(mpb, 1, value); }

		/// <summary>
		/// Sets the blue value of all colors in the MaterialPropertyBlock.
		/// </summary>
		public void SetBlue(MaterialPropertyBlock mpb, float value)		{ SetColorValue(mpb, 2, value); }

		/// <summary>
		/// Sets the alpha value of all colors in the MaterialPropertyBlock.
		/// </summary>
		public void SetAlpha(MaterialPropertyBlock mpb, float value)	{ SetColorValue(mpb, 3, value); }

		/// <summary>
		/// Sets the MaterialPropertyBlock to have the lerp of all properties between Materials a and b, at time t.
		/// </summary>
		public void LerpProperties(MaterialPropertyBlock mpb, Material a, Material b, float t)
		{
			//lerp all properties of each type
			for(int i = 0; i < ColorProperties.Length; i++)
			{
				ColorProperty cp = ColorProperties[i];
				mpb.SetColor(cp.Name, cp.Lerp(a, b, t));
			}

			for(int i = 0; i < FloatProperties.Length; i++)
			{
				FloatProperty fp = FloatProperties[i];
				mpb.SetFloat(fp.Name, fp.Lerp(a, b, t));
			}

			for(int i = 0; i < VectorProperties.Length; i++)
			{
				VectorProperty vp = VectorProperties[i];
				mpb.SetVector(vp.Name, vp.Lerp(a, b, t));
			}

			if(USE_TEXTURES)
			{
				//currently unsupported...
				for(int i = 0; i < TextureProperties.Length; i++)
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
					foreach(ShaderProperties sp in targets)
					{
						sp.SetPropertiesFromShader();
					}
				}

				base.OnInspectorGUI();
			}
		}
#endif
	}
}