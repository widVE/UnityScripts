using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.IO
{
    [CreateAssetMenu(fileName = nameof(DataFileTXT), menuName = nameof(DataFile) + "/" + nameof(DataFileTXT), order = WIDVEEditor.C_ORDER)]
    public class DataFileTXT : DataFileText
    {
        [SerializeField]
        [HideInInspector]
        string _extension = ".txt";

        public override string Extension => _extension;

#if UNITY_EDITOR
        protected override void DrawExtension(SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_extension)));
        }
#endif
    }
}