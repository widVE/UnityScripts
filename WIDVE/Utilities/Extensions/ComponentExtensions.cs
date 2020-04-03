using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Shortcut for GameObject.MarkSceneDirty().
        /// </summary>
        public static bool MarkSceneDirty(this Component component)
        {
            return component.gameObject.MarkSceneDirty();
        }
    }
}