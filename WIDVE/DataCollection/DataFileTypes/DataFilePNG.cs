using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace WIDVE.DataCollection
{
	[CreateAssetMenu(fileName = nameof(DataFilePNG), menuName = nameof(DataFile) + "/" + nameof(DataFilePNG), order = WIDVEEditor.C_ORDER)]
	public class DataFilePNG : DataFileBinary
	{
		public override string Extension => "png";

		/// <summary>
		/// Converts a Texture to a Texture2D using a very convoluted but necessary method.
		/// </summary>
		public static Texture2D ConvertTextureToTexture2D(Texture texture)
		{   
			//https://answers.unity.com/questions/1271693/reading-pixel-data-from-materialmaintexture-return.html
			//need to do it this way for the Texture2D to actually have the correct colors...
			Texture2D texture2D = null;

			if (texture)
			{   
				//create new texture2D
				texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

				//cache current active render texture
				RenderTexture i_renderTexture = RenderTexture.active;
				
				//create new render texture that matches the passed in texture
				RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 32);
				
				//blit texture colors into the render texture
				UnityEngine.Graphics.Blit(texture, renderTexture);
				
				//set the new render texture as the active render texture
				RenderTexture.active = renderTexture;
				
				//store render texture's colors in the texture2D
				texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
				
				//the texture2D is now ready
				texture2D.Apply();
				
				//restore original render texture
				RenderTexture.active = i_renderTexture;
			}

			return texture2D;
		}

		/// <summary>
		/// Load a Texture2D from this file.
		/// </summary>
		/// <returns>Texture2D if file exists, null otherwise.</returns>
		public Texture2D Load()
		{
			Texture2D texture2D = null;

			if (FileExists)
			{
				byte[] bytes = GetBytes();

				//initialize this just so it can be used on the next line (size isn't important)
				texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(bytes);
			}

			return texture2D;
		}

		/// <summary>
		/// Save a Texture to this file.
		/// </summary>
		public void Save(Texture texture)
		{
			Save(ConvertTextureToTexture2D(texture));
		}

		/// <summary>
		/// Save a Texture2D to this file.
		/// </summary>
		public void Save(Texture2D texture2D)
		{
			byte[] texBytes = texture2D.EncodeToPNG();
			Write(texBytes);
		}
	}
}