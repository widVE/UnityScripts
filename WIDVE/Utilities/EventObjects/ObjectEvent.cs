using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(ObjectEvent), menuName = WIDVEEditor.MENU + "/" + nameof(ObjectEvent), order = WIDVEEditor.C_ORDER)]
	public class ObjectEvent : ScriptableEvent<object> { }
}