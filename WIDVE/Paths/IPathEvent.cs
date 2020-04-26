using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Paths
{
    public interface IPathEvent
    {
        void Trigger(PathTrigger trigger);
    }
}