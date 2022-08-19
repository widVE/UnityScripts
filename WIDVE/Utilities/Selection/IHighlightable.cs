//Copyright WID Virtual Environments Group 2018-Present
//Simon Smith
//Ross Tredinnick

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public interface IHighlightable
    {
        void StartHighlight(Selector selector);

        void EndHighlight();
    }
}