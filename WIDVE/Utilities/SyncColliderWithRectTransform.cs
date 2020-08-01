using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Patterns;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Utilities
{
	[ExecuteAlways]
	public class SyncColliderWithRectTransform : MonoBehaviour
	{
		RectTransform _parentTransform;
		RectTransform ParentTransform => _parentTransform ? _parentTransform : (_parentTransform = transform as RectTransform);

		BoxCollider _syncedCollider;
		BoxCollider SyncedCollider => _syncedCollider ? _syncedCollider : (_syncedCollider = GetComponent<BoxCollider>());

		public void Sync()
		{
			if(!ParentTransform) return;
			if(!SyncedCollider) return;

			Vector3 newSize = new Vector3(ParentTransform.rect.width,
										  ParentTransform.rect.height,
										  SyncedCollider.size.z);
			SyncedCollider.size = newSize;
		}

		void OnEnable()
		{
			Sync();
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(SyncColliderWithRectTransform))]
		class Editor : UnityEditor.Editor
		{
			ICommand SHF;

			void SetHideFlags()
			{
				SyncColliderWithRectTransform scwrt = target as SyncColliderWithRectTransform;

				if(!scwrt.SyncedCollider) return;

				if(SHF != null) SHF.Undo();
				SHF = new UnityCommands.SetHideFlags(scwrt.SyncedCollider, HideFlags.NotEditable);
				SHF.Execute();
			}

			public override void OnInspectorGUI()
			{
				SyncColliderWithRectTransform scwrt = target as SyncColliderWithRectTransform;

				base.OnInspectorGUI();

				scwrt.Sync();

				//set hide flags on collider
				if(SHF == null)
				{
					SetHideFlags();
				}
			}

			void OnDisable()
			{
				if(SHF != null)
				{
					SHF.Undo();
					SHF = null;
				}
			}
		}
#endif
	}
}