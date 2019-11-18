﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
						Gizmos.DrawGUITexture(textureRect,
											  texture,
											  textureLeftBorder,
											  textureRightBorder,
											  textureTopBorder,
											  textureBottomBorder,
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

#if UNITY_EDITOR
		[CustomEditor(typeof(Gizmo))]
		public class Editor : UnityEditor.Editor
		{
			SerializedProperty Type;
			SerializedProperty Color;
			SerializedProperty Size;
			SerializedProperty UseWorldScale;
			SerializedProperty DrawWhenSelected;
			//line only
			SerializedProperty LineStart;
			SerializedProperty LineEnd;
			//frustum only
			SerializedProperty FrustumFOV;
			SerializedProperty FrustumMaxRange;
			SerializedProperty FrustumMinRange;
			SerializedProperty FrustumAspect;
			//icon only
			SerializedProperty IconFilename;
			SerializedProperty IconAllowScaling;
			//texture only
			SerializedProperty TextureRect;
			SerializedProperty Texture;
			SerializedProperty TextureMaterial;
			SerializedProperty TextureLeftBorder;
			SerializedProperty TextureRightBorder;
			SerializedProperty TextureTopBorder;
			SerializedProperty TextureBottomBorder;
			//mesh only
			SerializedProperty Mesh;
			SerializedProperty MeshPosition;
			SerializedProperty MeshRotation;

			void OnEnable()
			{
				Type = serializedObject.FindProperty(nameof(Gizmo.Type));
				Color = serializedObject.FindProperty(nameof(Gizmo.Color));
				Size = serializedObject.FindProperty(nameof(Gizmo.Size));
				UseWorldScale = serializedObject.FindProperty(nameof(Gizmo.UseWorldScale));
				DrawWhenSelected = serializedObject.FindProperty(nameof(Gizmo.DrawWhenSelected));
				//line
				LineStart = serializedObject.FindProperty(nameof(Gizmo.LineStart));
				LineEnd = serializedObject.FindProperty(nameof(Gizmo.LineEnd));
				//frustum
				FrustumFOV = serializedObject.FindProperty(nameof(Gizmo.FrustumFOV));
				FrustumMaxRange = serializedObject.FindProperty(nameof(Gizmo.FrustumMaxRange));
				FrustumMinRange = serializedObject.FindProperty(nameof(Gizmo.FrustumMinRange));
				FrustumAspect = serializedObject.FindProperty(nameof(Gizmo.FrustumAspect));
				//icon
				IconFilename = serializedObject.FindProperty(nameof(Gizmo.IconFilename));
				IconAllowScaling = serializedObject.FindProperty(nameof(Gizmo.IconAllowScaling));
				//texture
				TextureRect = serializedObject.FindProperty(nameof(Gizmo.TextureRect));
				Texture = serializedObject.FindProperty(nameof(Gizmo.Texture));
				TextureMaterial = serializedObject.FindProperty(nameof(Gizmo.TextureMaterial));
				TextureLeftBorder = serializedObject.FindProperty(nameof(Gizmo.TextureLeftBorder));
				TextureRightBorder = serializedObject.FindProperty(nameof(Gizmo.TextureRightBorder));
				TextureTopBorder = serializedObject.FindProperty(nameof(Gizmo.TextureTopBorder));
				TextureBottomBorder = serializedObject.FindProperty(nameof(Gizmo.TextureBottomBorder));
				//mesh
				Mesh = serializedObject.FindProperty(nameof(Gizmo.Mesh));
				MeshPosition = serializedObject.FindProperty(nameof(Gizmo.MeshPosition));
				MeshRotation = serializedObject.FindProperty(nameof(Gizmo.MeshRotation));
			}

			public override void OnInspectorGUI()
			{	//draw properties based on current type...
				serializedObject.Update();

				EditorGUILayout.PropertyField(Type);

				if (Type.enumValueIndex != (int)Types.Texture)
				{
					EditorGUILayout.PropertyField(Color);
				}

				if (Type.enumValueIndex != (int)Types.Texture &&
					Type.enumValueIndex != (int)Types.Line &&
					Type.enumValueIndex != (int)Types.Frustum &&
					Type.enumValueIndex != (int)Types.Icon)
				{
					EditorGUILayout.PropertyField(Size);
				}

				EditorGUILayout.PropertyField(UseWorldScale);
				EditorGUILayout.PropertyField(DrawWhenSelected);

				if (Type.enumValueIndex == (int)Types.Line)
				{
					EditorGUILayout.PropertyField(LineStart);
					EditorGUILayout.PropertyField(LineEnd);
				}

				if (Type.enumValueIndex == (int)Types.Frustum)
				{
					EditorGUILayout.PropertyField(FrustumFOV);
					EditorGUILayout.PropertyField(FrustumMinRange);
					EditorGUILayout.PropertyField(FrustumMaxRange);
					EditorGUILayout.PropertyField(FrustumAspect);
				}

				if (Type.enumValueIndex == (int)Types.Icon)
				{
					EditorGUILayout.PropertyField(IconFilename);
					EditorGUILayout.PropertyField(IconAllowScaling);
				}

				if (Type.enumValueIndex == (int)Types.Texture)
				{
					EditorGUILayout.PropertyField(Texture);
					EditorGUILayout.PropertyField(TextureMaterial);
					EditorGUILayout.PropertyField(TextureRect);
					EditorGUILayout.PropertyField(TextureLeftBorder);
					EditorGUILayout.PropertyField(TextureRightBorder);
					EditorGUILayout.PropertyField(TextureTopBorder);
					EditorGUILayout.PropertyField(TextureBottomBorder);
				}

				if (Type.enumValueIndex == (int)Types.Mesh || Type.enumValueIndex == (int)Types.WireMesh)
				{
					EditorGUILayout.PropertyField(Mesh);
					EditorGUILayout.PropertyField(MeshPosition);
					EditorGUILayout.PropertyField(MeshRotation);
				}

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}