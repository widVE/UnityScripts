using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public interface IHighlightable
    {
        //System.Action OnHighlightEnd { get; }

        void StartHighlight(Selector selector);

        void EndHighlight();
    }

    /*
    public static class IHighlightableExtensons
    {
        public static void InvokeOnHighlightEnd(this IHighlightable highlightable)
        {
            highlightable.OnHighlightEnd?.Invoke();
        }
    }
    */
}