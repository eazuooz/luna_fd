//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;

namespace UnityGameFramework.Editor
{
    // 콘솔에서 로그를 더블클릭하면 Unity 는 스택 맨 위 프레임을 연다. Log.Info 로 찍은 로그는
    // 맨 위가 언제나 DefaultLogHelper.cs 라서, 그대로 두면 매번 프레임워크 파일이 열린다.
    // 프레임워크의 로그 경로를 건너뛰고 "Log.Info 를 부른 줄" 을 열어 준다.
    //
    // 원본은 Unity 2019.1 부터 이 기능을 꺼 두었다(콘솔에 스택 링크가 생겼기 때문).
    // 이 에디션은 프레임워크가 Packages 아래에 있어 경로 처리를 다시 쓰고 되살렸다.
    internal static class LogRedirection
    {
        private static readonly Regex LogRegex = new Regex(@" \(at (.+)\:(\d+)\)\r?\n");

        private const string LogHelperMethodName = "UnityGameFramework.Runtime.DefaultLogHelper:Log";

        private static readonly string[] FrameworkLogFileNames = new string[]
        {
            "DefaultLogHelper.cs",
            "GameFrameworkLog.cs",
            "Log.cs"
        };

        // 에셋 식별자가 int(InstanceID) 에서 EntityId 로 바뀌면서 콜백 시그니처도 바뀌었다.
        // 맞지 않는 쪽을 쓰면 컴파일은 되지만 에셋을 열 때마다 에디터가 오류를 낸다.
        [OnOpenAsset(0)]
#if UNITY_6000_3_OR_NEWER
        private static bool OnOpenAsset(UnityEngine.EntityId entityId, int line)
#else
        private static bool OnOpenAsset(int instanceId, int line)
#endif
        {
            string selectedStackTrace = GetSelectedStackTrace();
            if (string.IsNullOrEmpty(selectedStackTrace))
            {
                return false;
            }

            string callerPath = null;
            int callerLine = 0;
            if (!TryFindCallerFrame(selectedStackTrace, out callerPath, out callerLine))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(callerPath);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            return InternalEditorUtility.OpenFileAtLineExternal(fullPath, callerLine);
        }

        internal static bool TryFindCallerFrame(string stackTrace, out string path, out int line)
        {
            path = null;
            line = 0;

            if (string.IsNullOrEmpty(stackTrace) || !stackTrace.Contains(LogHelperMethodName))
            {
                return false;
            }

            Match match = LogRegex.Match(stackTrace);
            if (!match.Success || !IsFrameworkLogFile(match.Groups[1].Value))
            {
                return false;
            }

            while (match.Success && IsFrameworkLogFile(match.Groups[1].Value))
            {
                match = match.NextMatch();
            }

            if (!match.Success)
            {
                return false;
            }

            path = match.Groups[1].Value;
            if (path.StartsWith("./"))
            {
                path = path.Substring(2);
            }

            return int.TryParse(match.Groups[2].Value, out line);
        }

        private static bool IsFrameworkLogFile(string path)
        {
            string fileName = Path.GetFileName(path);
            foreach (string frameworkLogFileName in FrameworkLogFileNames)
            {
                if (fileName == frameworkLogFileName)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetSelectedStackTrace()
        {
            Assembly editorWindowAssembly = typeof(EditorWindow).Assembly;
            if (editorWindowAssembly == null)
            {
                return null;
            }

            System.Type consoleWindowType = editorWindowAssembly.GetType("UnityEditor.ConsoleWindow");
            if (consoleWindowType == null)
            {
                return null;
            }

            FieldInfo consoleWindowFieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (consoleWindowFieldInfo == null)
            {
                return null;
            }

            EditorWindow consoleWindow = consoleWindowFieldInfo.GetValue(null) as EditorWindow;
            if (consoleWindow == null)
            {
                return null;
            }

            if (consoleWindow != EditorWindow.focusedWindow)
            {
                return null;
            }

            // Unity 내부 필드 이름이다. 패키지 규약(mXxx)과 무관하게 m_ActiveText 그대로여야 한다.
            FieldInfo activeTextFieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            if (activeTextFieldInfo == null)
            {
                return null;
            }

            return activeTextFieldInfo.GetValue(consoleWindow) as string;
        }
    }
}
