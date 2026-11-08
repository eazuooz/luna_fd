//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using UnityEditor;

namespace UnityGameFramework.Editor
{
    public static class LogScriptingDefineSymbols
    {
        private const string EnableLogScriptingDefineSymbol = "ENABLE_LOG";
        private const string EnableDebugAndAboveLogScriptingDefineSymbol = "ENABLE_DEBUG_AND_ABOVE_LOG";
        private const string EnableInfoAndAboveLogScriptingDefineSymbol = "ENABLE_INFO_AND_ABOVE_LOG";
        private const string EnableWarningAndAboveLogScriptingDefineSymbol = "ENABLE_WARNING_AND_ABOVE_LOG";
        private const string EnableErrorAndAboveLogScriptingDefineSymbol = "ENABLE_ERROR_AND_ABOVE_LOG";
        private const string EnableFatalAndAboveLogScriptingDefineSymbol = "ENABLE_FATAL_AND_ABOVE_LOG";
        private const string EnableDebugLogScriptingDefineSymbol = "ENABLE_DEBUG_LOG";
        private const string EnableInfoLogScriptingDefineSymbol = "ENABLE_INFO_LOG";
        private const string EnableWarningLogScriptingDefineSymbol = "ENABLE_WARNING_LOG";
        private const string EnableErrorLogScriptingDefineSymbol = "ENABLE_ERROR_LOG";
        private const string EnableFatalLogScriptingDefineSymbol = "ENABLE_FATAL_LOG";

        private static readonly string[] AboveLogScriptingDefineSymbols = new string[]
        {
            EnableDebugAndAboveLogScriptingDefineSymbol,
            EnableInfoAndAboveLogScriptingDefineSymbol,
            EnableWarningAndAboveLogScriptingDefineSymbol,
            EnableErrorAndAboveLogScriptingDefineSymbol,
            EnableFatalAndAboveLogScriptingDefineSymbol
        };

        private static readonly string[] SpecifyLogScriptingDefineSymbols = new string[]
        {
            EnableDebugLogScriptingDefineSymbol,
            EnableInfoLogScriptingDefineSymbol,
            EnableWarningLogScriptingDefineSymbol,
            EnableErrorLogScriptingDefineSymbol,
            EnableFatalLogScriptingDefineSymbol
        };

        [MenuItem("Game Framework/Log Scripting Define Symbols/Disable All Logs", false, 30)]
        public static void DisableAllLogs()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableLogScriptingDefineSymbol);

            foreach (string specifyLogScriptingDefineSymbol in SpecifyLogScriptingDefineSymbols)
            {
                ScriptingDefineSymbols.RemoveScriptingDefineSymbol(specifyLogScriptingDefineSymbol);
            }

            foreach (string aboveLogScriptingDefineSymbol in AboveLogScriptingDefineSymbols)
            {
                ScriptingDefineSymbols.RemoveScriptingDefineSymbol(aboveLogScriptingDefineSymbol);
            }
        }

        [MenuItem("Game Framework/Log Scripting Define Symbols/Enable All Logs", false, 31)]
        public static void EnableAllLogs()
        {
            DisableAllLogs();
            ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableLogScriptingDefineSymbol);
        }

        [MenuItem("Game Framework/Log Scripting Define Symbols/Enable Debug And Above Logs", false, 32)]
        public static void EnableDebugAndAboveLogs()
        {
            SetAboveLogScriptingDefineSymbol(EnableDebugAndAboveLogScriptingDefineSymbol);
        }

        [MenuItem("Game Framework/Log Scripting Define Symbols/Enable Info And Above Logs", false, 33)]
        public static void EnableInfoAndAboveLogs()
        {
            SetAboveLogScriptingDefineSymbol(EnableInfoAndAboveLogScriptingDefineSymbol);
        }

        [MenuItem("Game Framework/Log Scripting Define Symbols/Enable Warning And Above Logs", false, 34)]
        public static void EnableWarningAndAboveLogs()
        {
            SetAboveLogScriptingDefineSymbol(EnableWarningAndAboveLogScriptingDefineSymbol);
        }

        [MenuItem("Game Framework/Log Scripting Define Symbols/Enable Error And Above Logs", false, 35)]
        public static void EnableErrorAndAboveLogs()
        {
            SetAboveLogScriptingDefineSymbol(EnableErrorAndAboveLogScriptingDefineSymbol);
        }

        [MenuItem("Game Framework/Log Scripting Define Symbols/Enable Fatal And Above Logs", false, 36)]
        public static void EnableFatalAndAboveLogs()
        {
            SetAboveLogScriptingDefineSymbol(EnableFatalAndAboveLogScriptingDefineSymbol);
        }

        public static void SetAboveLogScriptingDefineSymbol(string aboveLogScriptingDefineSymbol)
        {
            if (string.IsNullOrEmpty(aboveLogScriptingDefineSymbol))
            {
                return;
            }

            foreach (string i in AboveLogScriptingDefineSymbols)
            {
                if (i == aboveLogScriptingDefineSymbol)
                {
                    DisableAllLogs();
                    ScriptingDefineSymbols.AddScriptingDefineSymbol(aboveLogScriptingDefineSymbol);
                    return;
                }
            }
        }

        public static void SetSpecifyLogScriptingDefineSymbols(string[] specifyLogScriptingDefineSymbols)
        {
            if (specifyLogScriptingDefineSymbols == null || specifyLogScriptingDefineSymbols.Length <= 0)
            {
                return;
            }

            bool removed = false;
            foreach (string specifyLogScriptingDefineSymbol in specifyLogScriptingDefineSymbols)
            {
                if (string.IsNullOrEmpty(specifyLogScriptingDefineSymbol))
                {
                    continue;
                }

                foreach (string i in SpecifyLogScriptingDefineSymbols)
                {
                    if (i == specifyLogScriptingDefineSymbol)
                    {
                        if (!removed)
                        {
                            removed = true;
                            DisableAllLogs();
                        }

                        ScriptingDefineSymbols.AddScriptingDefineSymbol(specifyLogScriptingDefineSymbol);
                        break;
                    }
                }
            }
        }
    }
}
