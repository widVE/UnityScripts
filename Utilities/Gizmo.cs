using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities.Extensions;

namespace WIDVE.Utilities
{
	public class Gizmo : MonoBehaviour
	{
		public enum Types { Sphere, Cube, WireSphere, WireCube, Ray, Line, Frustum, Icon, Texture, Mesh, WireMesh }
		[SerializeField]
		Types Type = Types.WireSphere;
		[SerializeField]
		Color Color = Color.blue;
		[SerializeField][Range(0.01f, 5f)]
		float Size = 1f;
		[SerializeField]
		bool UseWorldScale = true;
		[SerializeField]
		bool DrawWhenSelected = false;
		//line only
		[SerializeField]
		Vector3 LineStart = Vector3.zero;
		[SerializeField]
		Vector3 LineEnd = Vector3.forward;
		//frustum only
		[SerializeField]
		float FrustumFOV = 60f;
		[SerializeField]
		float FrustumMaxRange = 1f;
		[SerializeField]
		float FrustumMinRange = .1f;
		[SerializeField]
		float FrustumAspect = 1f;
		//icon only
		[SerializeField]
		string IconFilename = string.Empty;
		[SerializeField]
		bool IconAllowScaling = true;
		//texture only
		[SerializeField]
		Rect TextureRect = Rect.zero;
		[SerializeField]
		Texture Texture = null;
		[SerializeField]
		Material TextureMaterial = null;
		[SerializeField]
		int TextureLeftBorder = 0;
		[SerializeField]
		int TextureRightBorder = 0;
		[SerializeField]
		int TextureTopBorder = 0;
		[SerializeField]
		int TextureBottomBorder = 0;
		//mesh only
		[SerializeField]
		Mesh Mesh = null;
		[SerializeField]
		Vector3 MeshPosition = Vector3.zero;
		[SerializeField]
		Vector3 MeshRotation = Vector3.zero;

		void OnDrawGizmos()
		{
			if (!DrawWhenSelected)
			{
				Draw();
			}
		}

		void OnDrawGizmosSelected()
		{
			if (DrawWhenSelected)
			{
				Draw();
			}
		}

		/// <summary>
		/// Draw the gizmo.
		/// </summary>
		public void Draw()
		{
			Draw(transform, Color, Type, Size, UseWorldScale,
				 LineStart, LineEnd,
				 FrustumFOV, FrustumMaxRange, FrustumMinRange, FrustumAspect,
				 IconFilename, IconAllowScaling,
				 TextureRect, Texture, TextureMaterial,
				 TextureLeftBorder, TextureRightBorder, TextureTopBorder, TextureBottomBorder,
				 Mesh, MeshPosition, MeshRotation);
		}

		/// <summary>
		/// Draw a gizmo with the specified settings.
		/// </summary>
		public static void Draw(Transform parent, Color color, Types type, float size, bool useWorldScale=true,
								//line
								Vector3 lineStart=new Vector3(), Vector3 lineEnd=new Vector3(),
								//frustum
								float frustumFOV=60f, float frustumMaxRange=1f, float frustumMinRange=.1f, float frustumAspect=1f,
								//icon
								string iconFilename="", bool iconAllowScaling=true,
								//texture
								Rect textureRect=new Rect(), Texture texture=null, Material textureMaterial=null,
								int textureLeftBorder=0, int textureRightBorder = 0, int textureTopBorder = 0, int textureBottomBorder = 0,
								//mesh
								Mesh mesh=null, Vector3 meshPosition=new Vector3(), Vector3 meshRotation=new Vector3())
		{
			Gizmos.matrix = parent.localToWorldMatrix;
			Gizmos.color = color;
			float v_size_x = useWorldScale ? (size / parent.lossyScale.x) : size;
			float v_size_y = useWorldScale ? (size / parent.lossyScale.y) : size;
			float v_size_z = useWorldScale ? (size / parent.lossyScale.z) : size;
			float f_size = Mathf.Min(v_size_x, v_size_y, v_size_z);
			Vector3 v_size = new Vector3(v_size_x, v_size_y, v_size_z);
			switch (type)
			{
				case Types.Sphere:
					Gizmos.DrawSphere(Vector3.zero, f_size);
					break;
				case Types.WireSphere:
					Gizmos.DrawWireSphere(Vector3.zero, f_size);
					break;
				case Types.Cube:
					Gizmos.DrawCube(Vector3.zero, v_size);
					break;
				case Types.WireCube:
					Gizmos.DrawWireCube(Vector3.zero, v_size);
					break;
				case Types.Ray:
					Gizmos.DrawRay(Vector3.zero, Vector3.forward * f_size);
					break;
				case Types.Line:
					Gizmos.DrawLine(lineStart, lineEnd);
					break;
				case Types.Frustum:
					Gizmos.DrawFrustum(Vector3.zero, frustumFOV, frustumMaxRange, frustumMinRange, frustumAspect);
					break;
				case Types.Icon:
					if (!string.IsNullOrEmpty(iconFilename))
					{
						Gizmos.DrawIcon(Vector3.zero, iconFilename, iconAllowScaling);
					}
					break;
				case Types.Texture:
					if (texture != null && textureMaterial != null)
					{
						Gizmos.DrawGUITexture(textureRect, texture,
											  textureLeftBorder, textureRightBorder, textureTopBorder, textureBottomBorder,
											  textureMaterial);
					}
					break;
				case Types.Mesh:
					if (mesh != null)
					{
						Gizmos.DrawMesh(mesh, meshPosition, Quaternion.Euler(meshRotation), v_size);
					}
					break;
				case Types.WireMesh:
					if (mesh != null)
					{
						Gizmos.DrawWireMesh(mesh, meshPosition, Quaternion.Euler(meshRotation), v_size);
					}
					break;
				default:
					break;
			}
		}
	}
}