using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ReorderableListAttribute : PropertyAttribute
    {
        readonly string _listName;
        public string ListName => _listName;

        public ReorderableListAttribute(string listName)
        {
            _listName = listName;
        }
    }
}