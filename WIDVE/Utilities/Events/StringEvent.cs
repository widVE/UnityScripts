using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(StringEvent), menuName = WIDVEEditor.MENU + "/" + nameof(StringEvent), order = WIDVEEditor.C_ORDER)]
	public class StringEvent : ScriptableEvent<string> { }
}