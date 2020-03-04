using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(BoolEvent), menuName = WIDVEEditor.MENU + "/" + nameof(BoolEvent), order = WIDVEEditor.C_ORDER)]
	public class BoolEvent : ScriptableEvent<bool> { }
}