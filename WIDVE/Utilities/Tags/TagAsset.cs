using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    [CreateAssetMenu(fileName = nameof(TagAsset), menuName = nameof(Utilities) + "/" + nameof(TagAsset), order = WIDVEEditor.C_ORDER)]
    public class TagAsset : ScriptableObject, System.IEquatable<TagAsset>
    {
        [SerializeField]
        string _tag;
        public string Tag
        {
            get => _tag;
            private set => _tag = value;
        }

        public bool Equals(TagAsset other)
        {
            return Tag == other.Tag;
        }
    }
}