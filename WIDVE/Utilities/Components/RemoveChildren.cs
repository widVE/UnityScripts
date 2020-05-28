using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
    public class RemoveChildren : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Children this many levels down (or deeper) will be deleted.")]
        int _level = 1;
        int Level => _level;

        int Remove()
        {
            return RemoveAllChildren(transform, 0, Level);
        }

        static int RemoveAllChildren(Transform transform, int currentLevel, int levelToRemove)
        {
            if(currentLevel >= levelToRemove)
            {
                //remove this object
                DeleteObject(transform.gameObject);
                return 1;
            }
            else
            {
                //travel deeper
                currentLevel++;

                int childrenRemoved = 0;
                for(int i = transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = transform.GetChild(i);
                    childrenRemoved += RemoveAllChildren(child, currentLevel, levelToRemove);
                }

                return childrenRemoved;
            }
        }

        static void DeleteObject(GameObject gameObject)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(gameObject);
#else
            Object.DestroyImmediate(gameObject);
#endif
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(RemoveChildren))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if(GUILayout.Button("Remove Children"))
                {
                    Undo.SetCurrentGroupName($"Remove Children");

                    int totalChildrenRemoved = 0;

                    foreach(RemoveChildren rc in targets)
                    {
                        totalChildrenRemoved += rc.Remove();
                    }

                    if(totalChildrenRemoved > 0) Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }
            }
        }
#endif
    }
}