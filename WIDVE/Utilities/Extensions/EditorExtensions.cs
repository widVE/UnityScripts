using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

namespace WIDVE.Utilities
{
    public static class EditorExtensions
    {
		#region Rect extensions

		/// <summary>
		/// Returns a new Rect at the correct height with single line height.
		/// </summary>
		/// <param name="position">Starting position Rect from property drawer.</param>
		/// <param name="e">Element number (controls rect y position).</param>
		/// <param name="height">Height in lines.</param>
		public static Rect GetRect(this Rect position, ref int e, int height = 1)
		{
			Rect rect = new Rect(position.x,
							position.y + (EditorGUIUtility.singleLineHeight * e),
							position.width,
							EditorGUIUtility.singleLineHeight * height);
			e += height;
			return rect;
		}

		#endregion

		#region PropertyDrawer extensions

		/// <summary>
		/// Returns the base property of this SerializedProperty.
		/// </summary>
		public static SerializedProperty GetBaseProperty(this SerializedProperty property)
		{
			return property.serializedObject.FindProperty(property.propertyPath.Split('.')[0]);
		}

		/// <summary>
		/// Makes a new List from this SerializedProperty.
		/// </summary>
		/// <typeparam name="T">Type of Object inside the list.</typeparam>
		public static List<T> MakeList<T>(this SerializedProperty property) where T : Object
		{
			//https://answers.unity.com/questions/682932/using-generic-list-with-serializedproperty-inspect.html

			//only works on lists or arrays
			if(!property.isArray) return null;

			//iterate a copy of the property
			SerializedProperty p = property.Copy();

			//skip type field
			p.Next(true);

			//get length of list in property
			p.Next(true);
			int length = p.intValue;

			//read values into list
			p.Next(true);
			List<T> list = new List<T>(length);
			for(int i = 0; i < length; i++)
			{
				list.Add(p.objectReferenceValue as T);
				if(i < length - 1) p.Next(false);
			}

			return list;
		}

		/// <summary>
		/// Makes a new ReorderableList from this SerializedProperty.
		/// </summary>
		/// <param name="header">Header of the list displayed in the inspector.</param>
		public static ReorderableList MakeReorderableList(this SerializedProperty property, string header = "")
		{
			if(property == null || !property.isArray) return null;

			//make a reorderable list
			ReorderableList rList = new ReorderableList(property.serializedObject,
														property,
														true, true, true, true);

			if(header == string.Empty) header = property.displayName;
			rList.drawHeaderCallback = rect =>
			{
				EditorGUI.LabelField(rect, header);
			};

			rList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				SerializedProperty element = rList.serializedProperty.GetArrayElementAtIndex(index);
				EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element, true)),
										property: element,
										label: new GUIContent(index.ToString()));
			};

			return rList;
		}

		#endregion
	}
}
#endif