using System;
using UnityEngine;

namespace Luna.Gameplay
{
    public sealed class LunaEntityCatalog : ScriptableObject
    {
        [Serializable] public sealed class Entry
        {
            public GameObject Prefab;
            public string Address;
            public string LogicType;
            public string Group = "Actors";
        }
        public Entry[] Entries = Array.Empty<Entry>();
    }
}
