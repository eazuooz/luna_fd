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
    public sealed class UIIntKey : MonoBehaviour
    {
        [SerializeField]
        private int mKey = 0;

        public int Key
        {
            get
            {
                return mKey;
            }
            set
            {
                mKey = value;
            }
        }
    }
}
