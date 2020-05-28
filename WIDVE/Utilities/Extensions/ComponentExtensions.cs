using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Calls EditorUtility.SetDirty() on the specified Component.
        /// </summary>
        /// <returns>True if SetDirty was called, false otherwise.</returns>
        public static bool SetDirty(this Component component)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(component);
            return true;
#else
			return false;
#endif
        }

        /// <summary>
        /// Shortcut for GameObject.MarkSceneDirty().
        /// </summary>
        public static bool MarkSceneDirty(this Component component)
        {
            return component.gameObject.MarkSceneDirty();
        }
    }
}