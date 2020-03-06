using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace WIDVE.Patterns
{
	public abstract class Observer : MonoBehaviour
	{
		protected enum NotifyModes { Parent, Children, Custom }

		[SerializeField]
		NotifyModes _notifyMode = NotifyModes.Children;
		protected NotifyModes NotifyMode => _notifyMode;

		[SerializeField]
		[HideInInspector]
		List<GameObject> _customObservers;
		protected List<GameObject> CustomObservers => _customObservers ?? (_customObservers = new List<GameObject>());
		
		/// <summary>
		/// Name of the serialized field that holds the object being observed.
		/// </summary>
		protected abstract string ObserveeName { get; }

		public abstract void Notify();

		protected abstract void Subscribe(Object o);
	
		protected abstract void Unsubscribe(Object o);

		/// <summary>
		/// Subscribe during OnEnable.
		/// <para>Observers should have the ExecuteAlways attribute.</para>
		/// </summary>
		protected abstract void OnEnable();

		/// <summary>
		/// Unsubscribe during OnDisable.
		/// <para>Observers should have the ExecuteAlways attribute.</para>
		/// </summary>
		protected abstract void OnDisable();

#if UNITY_EDITOR
		[CustomEditor(typeof(Observer), true)]
		class Editor : UnityEditor.Editor
		{
			ReorderableList CustomObservers;

			void OnEnable()
			{
				CustomObservers = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(_customObservers)),
								true, true, true, true);

				CustomObservers.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Observers");
				};

				CustomObservers.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = CustomObservers.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
											property: element,
											label: new GUIContent(index.ToString()));
				};
			}

			public override void OnInspectorGUI()
			{
				Observer observer = target as Observer;
				SerializedProperty observee = serializedObject.FindProperty(observer.ObserveeName);
				SerializedProperty notifyMode = serializedObject.FindProperty(nameof(_notifyMode));

				//store the initial observed object
				Object i_object = observee.objectReferenceValue;

				serializedObject.Update();

				EditorGUI.BeginChangeCheck();

				DrawDefaultInspector();

				//show the custom observers if they are being used
				if(notifyMode.enumValueIndex == (int)NotifyModes.Custom)
				{
					CustomObservers.DoLayoutList();
				}

				//before applying any modifications, check if subscribed object has changed
				bool changed = EditorGUI.EndChangeCheck();

				if(changed)
				{
					//unsubscribe from old object
					observer.Unsubscribe(i_object);
				}

				serializedObject.ApplyModifiedProperties();

				if(changed)
				{
					//subscribe to the new object and notify that there was an update
					Object newObject = observee.objectReferenceValue;
					observer.Subscribe(newObject);
					observer.Notify();
				}
			}
		}
#endif
	}

	public abstract class Observer<T> : Observer where T : Object
	{
		IObserver<T>[] GetObservers()
		{
			switch(NotifyMode)
			{
				default:
				case NotifyModes.Children:
					//returns all observers in children
					return GetComponentsInChildren<IObserver<T>>(true);

				case NotifyModes.Parent:
					//returns the first observer in parent
					IObserver<T> parentObserver = GetComponentInParent<IObserver<T>>();
					if(parentObserver != null)
					{
						IObserver<T>[] parentObservers = { parentObserver };
						return parentObservers;
					}
					else return new IObserver<T>[0];

				case NotifyModes.Custom:
					//returns all observers attached to objects in the custom observers list
					List<IObserver<T>> customObservers = new List<IObserver<T>>(CustomObservers.Count);
					for(int i = 0; i < CustomObservers.Count; i++)
					{
						if(CustomObservers[i] == null) continue;
						IObserver<T>[] co = CustomObservers[i].GetComponents<IObserver<T>>();
						for(int j = 0; j < co.Length; j++)
						{
							customObservers.Add(co[j]);
						}
					}
					return customObservers.ToArray();
			}
		}

		/// <summary>
		/// Invoke OnNotify on all observers in children (or first observer in parent).
		/// </summary>
		public override void Notify()
		{
			IObserver<T>[] observers = GetObservers();
			for(int i = 0; i < observers.Length; i++)
			{
				IObserver<T> observer = observers[i];
				if(observer == null) continue;
				observer.OnNotify();
			}
		}

		protected override void Subscribe(Object o) { Subscribe(o as T); }

		protected override void Unsubscribe(Object o) { Unsubscribe(o as T); }

		/// <summary>
		/// Subscribe to the event that will call Notify.
		/// </summary>
		protected abstract void Subscribe(T t);

		/// <summary>
		/// Unsubscribe from the event that will call Notify.
		/// </summary>
		protected abstract void Unsubscribe(T t);
	}
}