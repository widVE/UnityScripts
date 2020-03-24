using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace WIDVE.DataCollection
{
	public class DataWriter : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		List<DataFile> _files;
		/// <summary>
		/// Data sent to the DataWriter will be written to these files.
		/// </summary>
		public List<DataFile> Files => _files ?? (_files = new List<DataFile>());

		Queue<DataContainer[]> _buffer;
		/// <summary>
		/// Data that has yet to be written.
		/// <para>Cleared at the beginning of each scheduled write.</para>
		/// </summary>
		Queue<DataContainer[]> Buffer => _buffer ?? (_buffer = new Queue<DataContainer[]>());

		readonly object BufferLock = new object();

		Thread WriteThread = null;

		bool WriteThreadShouldRun = false;

		[SerializeField]
		[Range(.1f, 10f)]
		[Tooltip("Time to sleep between threaded writes, in seconds.")]
		float _sleepTime = 1f;
		/// <summary>
		/// How much time to sleep between scheduled writes, in seconds.
		/// </summary>
		float SleepTime => _sleepTime;

		/// <summary>
		/// How much time to sleep between scheduled writes, in milliseconds.
		/// </summary>
		int SleepTimeMS => Mathf.CeilToInt(SleepTime * 1000);

		/// <summary>
		/// Add a set of data to the DataWriter.
		/// <para>Data will be written during the next scheduled write.</para>
		/// <para>Can block if the data buffer is being accessed from another thread.</para>
		/// </summary>
		/// <param name="data">Ordered array of DataContainers.</param>
		public void Add(DataContainer[] data)
		{
			lock (BufferLock)
			{
				Buffer.Enqueue(data);
			}
		}

		/// <summary>
		/// Trigger a write of the current data buffer and then sleep.
		/// </summary>
		void ThreadedWrite()
		{
			while (WriteThreadShouldRun)
			{
				WriteData();
				Thread.Sleep(SleepTimeMS);
			}
		}

		/// <summary>
		/// Writes the current stored data to each DataFile.
		/// </summary>
		void WriteData()
		{	
			//check that there is fresh data to write
			if(Buffer.Count > 0)
			{	
				//copy buffer so the same contents can be written to multiple files
				DataContainer[][] buffer;
				lock (BufferLock)
				{
					buffer = Buffer.ToArray();
					//clear buffer now...
					//...any data added while files are being written will get written the next time this function is called
					Buffer.Clear();
				}

				//for each file, write buffered data
				for (int i = 0; i < Files.Count; i++)
				{
					DataFile f = Files[i];
					f.WriteData(buffer);
				}
			}
		}

		/// <summary>
		/// Starts the writing thread.
		/// </summary>
		/// <returns>The newly-created thread.</returns>
		Thread StartWriteThread()
		{
			Thread writeThread = new Thread(new ThreadStart(ThreadedWrite));
			WriteThreadShouldRun = true;
			writeThread.Start();
			return writeThread;
		}

		/// <summary>
		/// Tells the writing thread to stop running and waits for it to join.
		/// </summary>
		void EndWriteThread()
		{
			WriteThreadShouldRun = false;

			if(WriteThread != null)
			{
				WriteThread.Join();
				WriteThread = null;
			}

			if(Buffer.Count > 0)
			{
				//write any final data
				WriteData();
			}
		}

		void OnEnable()
		{
			//store application data path before trying to access it from a thread...
			DataFile.StoreApplicationDataPaths();

			WriteThread = StartWriteThread();
		}

		void OnDisable()
		{
			EndWriteThread();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(DataWriter), true)]
		class Editor : UnityEditor.Editor
		{
			ReorderableList Files;

			protected virtual void OnEnable()
			{
				Files = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(_files)),
											true, true, true, true);

				Files.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Files");
				};

				Files.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = Files.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
											property: element,
											label: new GUIContent(index.ToString()));
				};
			}

			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				DrawDefaultInspector();
				Files.DoLayoutList();

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}