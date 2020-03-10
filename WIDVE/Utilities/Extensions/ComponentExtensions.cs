using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Uses EditorUtility to set the Object itself as dirty.
        /// </summary>
        /// <param name="unityObject">Unity Object that has been modified.</param>
        public static void SetDirty(this Object unityObject)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(unityObject);
#endif
        }
    }
}