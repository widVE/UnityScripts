using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
    public class PrefabSpawner : MonoBehaviour
    {
        [SerializeField]
        Object _prefab;
        protected Object Prefab => _prefab;

        protected string PrefabName => Prefab ? Prefab.name : nameof(Prefab);

        protected string PrefabTag => Prefab is GameObject go ? go.tag :
                                      Prefab is Component c ? c.gameObject.tag :
                                      string.Empty;

        public Object Spawn()
        {
            if(!Prefab) return null;

            return Spawn(transform, Prefab);
        }

        public Object[] SpawnMany(int amount)
        {
            if(!Prefab) return null;

            Object[] objects = new Object[amount];

            for(int i = 0; i < amount; i++)
            {
                objects[i] = Spawn();
            }

            return objects;
        }

        public static Object Spawn(Transform parent, Object prefab)
        {
            return parent.gameObject.InstantiatePrefab(prefab, true);
        }

        public void RemoveAllInstances()
        {
            if(!Prefab) return;

            for(int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform t = transform.GetChild(i);

                if(t.name == PrefabName && t.CompareTag(PrefabTag))
                {
                    DestroyImmediate(t.gameObject);
                }
            }
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(PrefabSpawner), true)]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                PrefabSpawner prefabSpawner = target as PrefabSpawner;

                if(GUILayout.Button($"Spawn {prefabSpawner.PrefabName}"))
                {
                    foreach(PrefabSpawner ps in targets)
                    {
                        Create(ps);
                    }
                }
            }

            static void Create(PrefabSpawner prefabSpawner)
            {
                if(!prefabSpawner.Prefab)
                {
                    Debug.Log($"[{prefabSpawner.name}] Error! Need a Prefab object.");
                    return;
                }

                Object spawned = prefabSpawner.Spawn();

                if(spawned) Undo.RegisterCreatedObjectUndo(spawned, $"Spawned {prefabSpawner.PrefabName}");
            }

            static void CreateMany(PrefabSpawner prefabSpawner, int amount)
            {
                if(!prefabSpawner.Prefab)
                {
                    Debug.Log($"[{prefabSpawner.name}] Error! Need a Prefab object.");
                    return;
                }

                Undo.SetCurrentGroupName($"Spawned {amount} {prefabSpawner.PrefabName}s");

                for(int i = 0; i < amount; i++)
                {
                    Create(prefabSpawner);
                }

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
        }
#endif
    }
}