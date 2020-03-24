using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace WIDVE.Utilities
{
    [CustomPropertyDrawer(typeof(ReorderableListAttribute))]
    public class ReorderableListPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ReorderableListAttribute rla = attribute as ReorderableListAttribute;

            int e = 0;
            SerializedProperty baseProperty = property.GetBaseProperty();
            EditorGUI.PropertyField(position.GetRect(ref e), property);
            EditorGUI.PropertyField(position.GetRect(ref e), baseProperty);


            return;

            /*
            if(property.isArray)
            {
                Rect r = GetRect(position, e++);
                EditorGUI.HelpBox(r, $"Drawing a ReorderableList for property of type [{property.type}].", MessageType.Error);

            }
            else
            {
                Rect r = GetRect(position, e++);
                EditorGUI.HelpBox(r, $"Can't draw ReorderableList! Property is of an invalid type [{property.type}].", MessageType.Error);
            }
            */
        }
    }
}
#endif