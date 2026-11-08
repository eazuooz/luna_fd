//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    public static class Help
    {
        [MenuItem("Game Framework/Documentation", false, 90)]
        public static void ShowDocumentation()
        {
            ShowHelp("https://www.yamyamcoding.com/389de3e1-fced-463f-873a-bcd7b40c4ed6");
        }

        private static void ShowHelp(string uri)
        {
            Application.OpenURL(uri);
        }
    }
}
