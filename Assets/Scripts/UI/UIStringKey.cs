//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class UIStringKey : MonoBehaviour
    {
        [SerializeField]
        private string mKey = null;

        public string Key
        {
            get
            {
                return mKey ?? string.Empty;
            }
            set
            {
                mKey = value;
            }
        }
    }
}
