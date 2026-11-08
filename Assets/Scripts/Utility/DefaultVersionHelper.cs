//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class DefaultVersionHelper : Version.IVersionHelper
    {
        public string GameVersion
        {
            get
            {
                return Application.version;
            }
        }

        public int InternalGameVersion
        {
            get
            {
                return 0;
            }
        }
    }
}
