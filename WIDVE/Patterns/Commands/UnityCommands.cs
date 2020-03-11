using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Patterns
{
	public static class UnityCommands
	{
		public class Enable : Command<Behaviour>
		{
			readonly bool Enabled;
			bool i_Enabled;

			public Enable(Behaviour target, bool enabled) : base(target)
			{
				Enabled = enabled;
			}

			public override void Execute()
			{
				if(!Target) return;

				i_Enabled = Target.enabled;
				Target.enabled = Enabled;
			}

			public override void Undo()
			{
				if(!Target) return;

				Target.enabled = i_Enabled;
			}
		}

		public class SetActive : Command<GameObject>
		{
			readonly bool Active;
			bool i_Active;

			public SetActive(GameObject target, bool enabled) : base(target)
			{
				Active = enabled;
			}

			public override void Execute()
			{
				if(!Target) return;

				i_Active = Target.activeSelf;
				Target.SetActive(Active);
			}

			public override void Undo()
			{
				if(!Target) return;

				Target.SetActive(i_Active);
			}
		}

		public class SetHideFlags : Command<Object>
		{
			readonly HideFlags Flags;
			HideFlags i_Flags;

			public SetHideFlags(Object target, HideFlags flags) : base(target)
			{
				Flags = flags;
			}

			public override void Execute()
			{
				if(!Target) return;

				i_Flags = Target.hideFlags;
				Target.hideFlags = Flags;
			}

			public override void Undo()
			{
				if(!Target) return;

				Target.hideFlags = i_Flags;
			}
		}
	}
}